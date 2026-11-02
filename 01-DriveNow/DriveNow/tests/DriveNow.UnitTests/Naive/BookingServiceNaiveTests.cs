using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace DriveNow.UnitTests.Naive;

public class BookingServiceNaiveTests
{
    [Fact]
    public void CreateBooking_ValidRequest_CreatesBookingSuccessfully()
    {
        // Arrange
        var service = new BookingService();
        
        var pickupAt = DateTime.Now.AddHours(10);
        var request = new BookingRequest
        {
            CustomerId = 2,
            VehicleId = 1,
            PickupAt =  pickupAt,
            ReturnAt = pickupAt.AddDays(3),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0
        };

        // Act
        var booking = service.CreateBooking(request);

        // Assert
        booking.Should().NotBeNull();
        booking.Status.Should().Be(BookingStatus.Reserved);
        booking.CustomerId.Should().Be(2);
        booking.VehicleId.Should().Be(1);
        booking.Deposit.Should().Be(1000m);
    }

    [Fact]
    public void CreateBooking_PlatinumCustomer_WaivesDeposit()
    {
        // Arrange
        var service = new BookingService();

        var pickupAt = DateTime.Now.AddHours(10);

        var request = new BookingRequest
        {
            CustomerId = 5,
            VehicleId = 2,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(3),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0
        };

        // Act
        var booking = service.CreateBooking(request);

        // Assert
        booking.Should().NotBeNull();
        booking.Status.Should().Be(BookingStatus.Reserved);
        booking.CustomerId.Should().Be(5);
        booking.VehicleId.Should().Be(2);
        booking.Deposit.Should().Be(0m);
    }

    [Fact]
    public void CreateBooking_OverlappingDates_ThrowsBookingRejectedException()
    {
        // Arrange
        var service = new BookingService();

        var pickupAt = DateTime.Now.AddDays(4);

        var request = new BookingRequest
        {
            CustomerId = 2,
            VehicleId = 5,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(3),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0
        };

        // Act
        Action act = () => service.CreateBooking(request);

        // Assert
        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Vehicle is already booked for these dates");
    }
}