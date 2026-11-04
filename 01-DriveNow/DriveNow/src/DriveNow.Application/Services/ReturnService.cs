
using DriveNow.Application.Abstractions;
using DriveNow.Application.Receipts;
using DriveNow.Application.Repositories;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.Services;
using DriveNow.Domain.ValueObjects;

namespace DriveNow.Application.Services;

public class ReturnService
{
    private readonly IBookingRepository _bookings;
    private readonly IVehicleRepository _vehicles;
    private readonly ICustomerRepository _customers;
    private readonly IPaymentGateway _payments;
    private readonly IEmailSender _email;
    private readonly IAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly ReceiptService _receiptService;

    public ReturnService(
        IBookingRepository bookings,
        IVehicleRepository vehicles,
        ICustomerRepository customers,
        IPaymentGateway payments,
        IEmailSender email,
        IAuditLog auditLog,
        TimeProvider timeProvider,
        ReceiptService receiptService)
    {
        _bookings = bookings;
        _vehicles = vehicles;
        _customers = customers;
        _payments = payments;
        _email = email;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _receiptService = receiptService;
    }

    public ReturnReport ProcessReturn(
        int bookingId,
        int endMileage,
        int fuelQuarters,
        decimal damageCost)
    {
        var booking = _bookings.GetById(bookingId)
            ?? throw new BookingNotFoundException($"Booking #{bookingId} not found");

        if (booking.Status != BookingStatus.Active)
            throw new InvalidBookingStateException("Only active bookings can be returned");

        if (endMileage < booking.StartMileage)
            throw new ArgumentException("End mileage cannot be below the start mileage");

        if (fuelQuarters is < 0 or > 4)
            throw new ArgumentException("Fuel quarters must be between 0 and 4");

        var vehicle = _vehicles.GetById(booking.VehicleId)
            ?? throw new VehicleNotFoundException($"Vehicle #{booking.VehicleId} not found");

        var customer = _customers.GetById(booking.CustomerId)
            ?? throw new CustomerNotFoundException($"Customer #{booking.CustomerId} not found");

        DateTime now = _timeProvider.GetLocalNow().DateTime;

        var report = BuildReport(
            booking,
            vehicle,
            endMileage,
            fuelQuarters,
            damageCost,
            now);

        booking.Status = BookingStatus.Completed;
        booking.ActualReturnAt = now;
        booking.EndMileage = endMileage;
        booking.ExtraCharges = report.TotalExtra;
        _bookings.Update(booking);

        vehicle.Mileage = endMileage;
        _vehicles.Update(vehicle);

        customer.CompletedRentals++;
        LoyaltyService.UpgradeTier(customer);
        _customers.Update(customer);

        if (report.TotalExtra > 0)
            _payments.Charge(customer.Id, report.TotalExtra, $"Extras {booking.Reference}");

        if (booking.Deposit > 0)
            _payments.Refund(customer.Id, booking.Deposit, $"Deposit refund {booking.Reference}");

        string receipt = _receiptService.Build(booking, report);

        _email.Send(customer.Email, $"Receipt {booking.Reference}", receipt);

        _auditLog.Write($"Booking {booking.Reference} returned");

        return report;
    }

    private static ReturnReport BuildReport(
        Booking booking,
        Vehicle vehicle,
        int endMileage,
        int fuelQuarters,
        decimal damageCost,
        DateTime now)
    {
        int lateMinutes = now <= booking.ReturnAt
            ? 0
            : (int)Math.Floor((now - booking.ReturnAt).TotalMinutes);

        decimal lateFee = LateFee(lateMinutes, vehicle.DailyRate);

        int allowedKm = 250 * booking.Price.RentalDays;
        int driven = endMileage - booking.StartMileage;
        int extraKm = Math.Max(0, driven - allowedKm);

        decimal perKm = vehicle.Category == VehicleCategory.Luxury ? 1.50m : 0.75m;
        decimal mileageFee = Round(extraKm * perKm);

        int missingFuel = 4 - fuelQuarters;

        decimal perQuarter = vehicle.Category is VehicleCategory.SUV or VehicleCategory.Van
            ? 300m
            : 200m;

        decimal fuelFee = Round(missingFuel * perQuarter);

        decimal deductible = booking.InsurancePlan switch
        {
            InsurancePlan.Basic => 10000m,
            InsurancePlan.Standard => 3000m,
            _ => 0m
        };

        decimal damageCharge = Round(Math.Min(damageCost, deductible));

        return new ReturnReport
        {
            LateMinutes = lateMinutes,
            LateFee = lateFee,
            AllowedKm = allowedKm,
            ExtraKm = extraKm,
            MileageFee = mileageFee,
            MissingFuelQuarters = missingFuel,
            FuelFee = fuelFee,
            DamageCharge = damageCharge,
            TotalExtra = Round(lateFee + mileageFee + fuelFee + damageCharge)
        };
    }

    private static decimal LateFee(int lateMinutes, decimal dailyRate)
    {
        if (lateMinutes > 240)
        {
            int periods = (int)Math.Ceiling(lateMinutes / 1440d);
            return Round(dailyRate * Math.Max(periods, 1));
        }

        if (lateMinutes >= 60)
        {
            int startedHours = lateMinutes % 60 == 0
                ? lateMinutes / 60
                : lateMinutes / 60 + 1;

            return Round(dailyRate * 0.10m * startedHours);
        }

        return 0m;
    }

    private static decimal Round(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
