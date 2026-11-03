
using DriveNow.Application.Abstractions;
using DriveNow.Application.Models;
using DriveNow.Application.Repositories;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.Calculations;
using DriveNow.Domain.ValueObjects;

namespace DriveNow.Application.Services;

public class BookingService
{
    private readonly ICustomerRepository _customers;
    private readonly IVehicleRepository _vehicles;
    private readonly IBookingRepository _bookings;
    private readonly IPromoCodeRepository _promos;
    private readonly PricingService _pricing;
    private readonly IPaymentGateway _payments;
    private readonly ISmsSender _sms;
    private readonly IEmailSender _email;
    private readonly IAuditLog _auditLog;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly TimeProvider _timeProvider;

    public BookingService(
        ICustomerRepository customers,
        IVehicleRepository vehicles,
        IBookingRepository bookings,
        IPromoCodeRepository promos,
        PricingService pricing,
        IPaymentGateway payments,
        ISmsSender sms,
        IEmailSender email,
        IAuditLog auditLog,
        IReferenceGenerator referenceGenerator,
        TimeProvider timeProvider)
    {
        _customers = customers;
        _vehicles = vehicles;
        _bookings = bookings;
        _promos = promos;
        _pricing = pricing;
        _payments = payments;
        _sms = sms;
        _email = email;
        _auditLog = auditLog;
        _referenceGenerator = referenceGenerator;
        _timeProvider = timeProvider;
    }

    public Booking CreateBooking(BookingRequest request)
    {
        DateTime now = _timeProvider.GetLocalNow().DateTime;

        Customer customer = RequireCustomer(request.CustomerId);
        int age = RentalMath.CalculateAge(customer.DateOfBirth, request.PickupAt);
        int licenseYears = RentalMath.CalculateLicenseYears(customer.LicenseIssueDate, request.PickupAt);
        RejectCustomer(customer, age, licenseYears);

        Vehicle vehicle = _vehicles.GetById(request.VehicleId)
            ?? throw new VehicleNotFoundException($"Vehicle #{request.VehicleId} not found");

        RejectVehicle(vehicle, age, licenseYears, request, now);

        if (request.AdditionalDrivers is < 0 or > 3)
            throw new BookingRejectedException("Up to 3 additional drivers allowed");

        if (_bookings.GetOpenBookingsForVehicle(vehicle.Id).Any(existing =>
                request.PickupAt < existing.ReturnAt && request.ReturnAt > existing.PickupAt))
            throw new BookingRejectedException("Vehicle is already booked for these dates");

        if (_bookings.CountOpenBookingsForCustomer(customer.Id) >= 2)
            throw new BookingRejectedException("Customer already has 2 active bookings");

        PriceBreakdown price = _pricing.Calculate(customer, vehicle, request);

        Booking booking = new Booking
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = request.PickupAt,
            ReturnAt = request.ReturnAt,
            InsurancePlan = request.InsurancePlan,
            AdditionalDrivers = request.AdditionalDrivers,
            PromoCode = request.PromoCode,
            Price = price,
            Status = BookingStatus.Reserved,
            CreatedAt = now,
            StartMileage = vehicle.Mileage,
            Reference = "DN-" + request.PickupAt.ToString("yyyyMMdd") + "-" + _referenceGenerator.Generate()
        };

        booking.Deposit = DepositFor(vehicle.Category, request.InsurancePlan, customer.LoyaltyTier);

        if (booking.Deposit > 0)
            _payments.Charge(customer.Id, booking.Deposit, $"Deposit {booking.Reference}");

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            PromoCode? promo = _promos.GetByCode(request.PromoCode);

            if (promo is not null)
            {
                promo.UsedCount++;
                _promos.Update(promo);
            }
        }

        _bookings.Add(booking);

        string note = $"Booking {booking.Reference} confirmed";

        _sms.Send(customer.Phone, note);
        _email.Send(customer.Email, note, note);
        _auditLog.Write($"Booking {booking.Reference} created");

        return booking;
    }

    public void StartRental(int bookingId)
    {
        DateTime now = _timeProvider.GetLocalNow().DateTime;

        Booking booking = _bookings.GetById(bookingId)
            ?? throw new BookingNotFoundException($"Booking #{bookingId} not found");

        if (booking.Status != BookingStatus.Reserved)
            throw new InvalidBookingStateException("Only reserved bookings can be started");

        if (now < booking.PickupAt.AddHours(-1))
            throw new InvalidBookingStateException("Too early to start this rental");

        booking.Status = BookingStatus.Active;

        _bookings.Update(booking);
        _auditLog.Write($"Booking {booking.Reference} started");
    }

    private Customer RequireCustomer(int customerId)
    {
        Customer customer = _customers.GetById(customerId)
            ?? throw new CustomerNotFoundException($"Customer #{customerId} not found");

        return customer;
    }

    private static void RejectCustomer(Customer customer, int age, int licenseYears)
    {
        if (customer.IsBlacklisted)
            throw new BookingRejectedException("Customer is blacklisted");

        if (age < 21)
            throw new BookingRejectedException("Driver must be at least 21");

        if (licenseYears < 1)
            throw new BookingRejectedException("License must be held for at least 1 year");
    }

    private static void RejectVehicle(
        Vehicle vehicle,
        int age,
        int licenseYears,
        BookingRequest request,
        DateTime now)
    {
        if (!vehicle.IsAvailable)
            throw new BookingRejectedException("Vehicle is not available");

        if (vehicle.Category == VehicleCategory.Luxury && !(age >= 25 && licenseYears >= 3))
            throw new BookingRejectedException("Luxury vehicles require age 25+ and 3 years of license");

        if (vehicle.Category == VehicleCategory.Van && licenseYears < 2)
            throw new BookingRejectedException("Vans require 2 years of license");

        if (request.PickupAt < now.AddHours(2))
            throw new BookingRejectedException("Pickup must be at least 2 hours from now");

        if (request.ReturnAt <= request.PickupAt ||
            request.ReturnAt - request.PickupAt > TimeSpan.FromDays(30))
            throw new BookingRejectedException("Rental period is invalid");
    }

    private static decimal DepositFor(
        VehicleCategory category,
        InsurancePlan insurance,
        LoyaltyTier tier)
    {
        if (tier == LoyaltyTier.Platinum)
            return 0m;

        decimal deposit = category switch
        {
            VehicleCategory.Economy or VehicleCategory.Compact => 1000m,
            VehicleCategory.SUV or VehicleCategory.Van => 2500m,
            VehicleCategory.Luxury => 5000m,
            _ => 0m
        };

        if (insurance == InsurancePlan.Premium)
            deposit = Math.Round(deposit / 2m, 2, MidpointRounding.AwayFromZero);

        return deposit;
    }
}
