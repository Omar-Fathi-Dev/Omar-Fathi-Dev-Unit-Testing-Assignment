using DriveNow.Application.Abstractions;
using DriveNow.Application.Receipts;
using DriveNow.Application.Repositories;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.ValueObjects;
using Moq;

public class ReturnServiceTests
{
    private static readonly DateTime PlannedReturn =
        new(2026, 11, 10, 10, 0, 0);

    [Theory]
    [InlineData(59, 0)]
    [InlineData(60, 100)]
    [InlineData(61, 200)]
    [InlineData(239, 400)]
    [InlineData(240, 400)]
    [InlineData(241, 1000)]
    [InlineData(1439, 1000)]
    [InlineData(1440, 1000)]
    [InlineData(1441, 2000)]
    [InlineData(2879, 2000)]
    [InlineData(2880, 2000)]
    [InlineData(2881, 3000)]
    [Trait("Rule", "BR-41")]
    public void ProcessReturn_LateByBoundaryMinutes_CalculatesLateFee(
        int lateMinutes,
        decimal expectedFee)
    {
        ReturnFixture fixture = new();
        fixture.SetNow(PlannedReturn.AddMinutes(lateMinutes));

        ReturnReport report =
            fixture.Sut.ProcessReturn(1, 50000, 4, 0m);

        Assert.Equal(lateMinutes, report.LateMinutes);
        Assert.Equal(expectedFee, report.LateFee);
    }

    [Fact]
    [Trait("Rule", "BR-41")]
    public void ProcessReturn_EarlyReturn_DoesNotChargeLateFee()
    {
        ReturnFixture fixture = new();
        fixture.SetNow(PlannedReturn.AddMinutes(-1));

        ReturnReport report =
            fixture.Sut.ProcessReturn(1, 50000, 4, 0m);

        Assert.Equal(0, report.LateMinutes);
        Assert.Equal(0m, report.LateFee);
    }

    [Theory]
    [InlineData(VehicleCategory.Economy, 0.75)]
    [InlineData(VehicleCategory.Luxury, 1.50)]
    [Trait("Rule", "BR-42")]
    public void ProcessReturn_ExtraMileage_UsesCategoryRate(
        VehicleCategory category,
        decimal rate)
    {
        ReturnFixture fixture = new();
        fixture.Vehicle.Category = category;
        fixture.Booking.Price.RentalDays = 2;

        ReturnReport report =
            fixture.Sut.ProcessReturn(1, 50501, 4, 0m);

        Assert.Equal(1, report.ExtraKm);
        Assert.Equal(rate, report.MileageFee);
    }

    [Theory]
    [InlineData(VehicleCategory.Economy, 200)]
    [InlineData(VehicleCategory.SUV, 300)]
    [InlineData(VehicleCategory.Van, 300)]
    [Trait("Rule", "BR-43")]
    public void ProcessReturn_MissingFuel_UsesCategoryRate(
        VehicleCategory category,
        decimal expectedFee)
    {
        ReturnFixture fixture = new();
        fixture.Vehicle.Category = category;

        ReturnReport report =
            fixture.Sut.ProcessReturn(1, 50000, 3, 0m);

        Assert.Equal(1, report.MissingFuelQuarters);
        Assert.Equal(expectedFee, report.FuelFee);
    }

    [Theory]
    [InlineData(InsurancePlan.Basic, 15000, 10000)]
    [InlineData(InsurancePlan.Standard, 5000, 3000)]
    [InlineData(InsurancePlan.Premium, 5000, 0)]
    [Trait("Rule", "BR-44")]
    public void ProcessReturn_DamageCost_IsCappedByInsuranceDeductible(
        InsurancePlan plan,
        decimal damage,
        decimal expectedCharge)
    {
        ReturnFixture fixture = new();
        fixture.Booking.InsurancePlan = plan;

        ReturnReport report =
            fixture.Sut.ProcessReturn(1, 50000, 4, damage);

        Assert.Equal(expectedCharge, report.DamageCharge);
    }

    [Fact]
    [Trait("Rule", "BR-45")]
    public void ProcessReturn_EarlyReturn_DoesNotRefundUnusedRentalDays()
    {
        ReturnFixture fixture = new();
        fixture.SetNow(PlannedReturn.AddDays(-1));
        fixture.Booking.Deposit = 500m;

        fixture.Sut.ProcessReturn(1, 50000, 4, 0m);

        Assert.Equal(2, fixture.Booking.Price.RentalDays);

        fixture.Payments.Verify(
            payment => payment.Refund(
                1,
                500m,
                "Deposit refund DN-TEST"),
            Times.Once);
    }

    [Fact]
    [Trait("Rule", "BR-46")]
    public void ProcessReturn_ValidReturn_UpdatesEntitiesAndSendsReceipt()
    {
        ReturnFixture fixture = new();
        fixture.Booking.Deposit = 500m;
        fixture.Customer.CompletedRentals = 4;

        fixture.Sut.ProcessReturn(1, 50000, 4, 0m);

        Assert.Equal(BookingStatus.Completed, fixture.Booking.Status);
        Assert.Equal(fixture.Now, fixture.Booking.ActualReturnAt);
        Assert.Equal(50000, fixture.Booking.EndMileage);
        Assert.Equal(50000, fixture.Vehicle.Mileage);
        Assert.Equal(5, fixture.Customer.CompletedRentals);
        Assert.Equal(LoyaltyTier.Silver, fixture.Customer.LoyaltyTier);

        fixture.Payments.Verify(
            payment => payment.Refund(
                1,
                500m,
                "Deposit refund DN-TEST"),
            Times.Once);

        fixture.Email.Verify(
            email => email.Send(
                "customer@example.com",
                "Receipt DN-TEST",
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void ProcessReturn_BookingDoesNotExist_ThrowsBookingNotFoundException()
    {
        ReturnFixture fixture = new();
        fixture.BookingExists = false;

        Assert.Throws<BookingNotFoundException>(
            () => fixture.Sut.ProcessReturn(1, 50000, 4, 0m));
    }

    [Fact]
    public void ProcessReturn_NonActiveBooking_ThrowsInvalidBookingStateException()
    {
        ReturnFixture fixture = new();
        fixture.Booking.Status = BookingStatus.Reserved;

        Assert.Throws<InvalidBookingStateException>(
            () => fixture.Sut.ProcessReturn(1, 50000, 4, 0m));
    }

    [Theory]
    [InlineData(49999, 4)]
    [InlineData(50000, -1)]
    [InlineData(50000, 5)]
    public void ProcessReturn_InvalidMileageOrFuel_ThrowsArgumentException(
        int mileage,
        int fuel)
    {
        ReturnFixture fixture = new();

        Assert.Throws<ArgumentException>(
            () => fixture.Sut.ProcessReturn(1, mileage, fuel, 0m));
    }

    private sealed class ReturnFixture
    {
        public readonly Mock<IBookingRepository> Bookings = new();
        public readonly Mock<IVehicleRepository> Vehicles = new();
        public readonly Mock<ICustomerRepository> Customers = new();
        public readonly Mock<IPaymentGateway> Payments = new();
        public readonly Mock<IEmailSender> Email = new();
        public readonly Mock<IAuditLog> Audit = new();

        public readonly Mock<ILineItemFactory> LineItems = new();
        public readonly Mock<ITaxCalculator> Tax = new();
        public readonly Mock<ICurrencyFormatter> Currency = new();
        public readonly Mock<IReceiptFormatter> Formatter = new();

        public bool BookingExists { get; set; } = true;

        public readonly Booking Booking = new()
        {
            Id = 1,
            Reference = "DN-TEST",
            CustomerId = 1,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
            ReturnAt = PlannedReturn,
            Status = BookingStatus.Active,
            StartMileage = 50000,
            Deposit = 0m,
            Price = new PriceBreakdown { RentalDays = 2 },
            InsurancePlan = InsurancePlan.Basic
        };

        public readonly Vehicle Vehicle = new()
        {
            Id = 1,
            Category = VehicleCategory.Economy,
            DailyRate = 1000m,
            Mileage = 50000
        };

        public readonly Customer Customer = new()
        {
            Id = 1,
            Email = "customer@example.com",
            Phone = "01000000000",
            LoyaltyTier = LoyaltyTier.None
        };

        public DateTime Now { get; private set; } = PlannedReturn;

        public ReturnService Sut
        {
            get
            {
                LineItems
                    .Setup(factory => factory.Create(
                        It.IsAny<Booking>(),
                        It.IsAny<ReturnReport?>()))
                    .Returns(new List<LineItem>());

                Tax.Setup(calculator =>
                    calculator.CalculateVat(It.IsAny<decimal>()))
                    .Returns(0m);

                Currency.Setup(formatter =>
                    formatter.Format(It.IsAny<decimal>()))
                    .Returns<decimal>(amount => amount.ToString("0.00"));

                Formatter.Setup(formatter => formatter.Render(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<string>>()))
                    .Returns("receipt");

                ReceiptService receipt = new(
                    LineItems.Object,
                    Tax.Object,
                    Currency.Object,
                    Formatter.Object);

                Bookings.Setup(repository => repository.GetById(1))
                    .Returns(() => BookingExists ? Booking : null);

                Vehicles.Setup(repository => repository.GetById(1))
                    .Returns(Vehicle);

                Customers.Setup(repository => repository.GetById(1))
                    .Returns(Customer);

                DateTimeOffset localTime = new(
                    DateTime.SpecifyKind(Now, DateTimeKind.Unspecified),
                    TimeZoneInfo.Local.GetUtcOffset(Now));

                TimeProvider clock = new FixedTimeProvider(localTime.ToUniversalTime());

                return new ReturnService(
                    Bookings.Object,
                    Vehicles.Object,
                    Customers.Object,
                    Payments.Object,
                    Email.Object,
                    Audit.Object,
                    clock,
                    receipt);
            }
        }

        public void SetNow(DateTime now) => Now = now;
    }
}

public class CancellationServiceTests
{
    private static readonly DateTime Pickup =
        new(2026, 11, 10, 10, 0, 0);

    [Theory]
    [InlineData(48, 1, 0)]
    [InlineData(48, 0, 0)]
    [InlineData(47, 59, 500)]
    [InlineData(24, 1, 500)]
    [InlineData(24, 0, 500)]
    [InlineData(23, 59, 1000)]
    [Trait("Rule", "BR-51")]
    public void Cancel_TimeBeforePickupBoundary_CalculatesCancellationFee(
        int hours,
        int minutes,
        decimal expectedFee)
    {
        CancellationFixture fixture = new();
        fixture.SetNow(Pickup.AddHours(-hours).AddMinutes(-minutes));

        decimal fee = fixture.Sut.Cancel(1);

        Assert.Equal(expectedFee, fee);
    }

    [Fact]
    [Trait("Rule", "BR-51")]
    public void Cancel_PlatinumCustomer_DoesNotPayCancellationFee()
    {
        CancellationFixture fixture = new();
        fixture.Customer.LoyaltyTier = LoyaltyTier.Platinum;
        fixture.SetNow(Pickup.AddHours(-1));

        decimal fee = fixture.Sut.Cancel(1);

        Assert.Equal(0m, fee);
    }

    [Fact]
    [Trait("Rule", "BR-53")]
    public void Cancel_ReservedBooking_UpdatesStatusPromoAndSendsNotifications()
    {
        CancellationFixture fixture = new();
        fixture.Booking.PromoCode = "SAVE10";

        fixture.Promos
            .Setup(repository => repository.GetByCode("SAVE10"))
            .Returns(new PromoCode
            {
                Code = "SAVE10",
                UsedCount = 2
            });

        fixture.Sut.Cancel(1);

        Assert.Equal(BookingStatus.Cancelled, fixture.Booking.Status);

        fixture.Promos.Verify(
            repository => repository.Update(
                It.Is<PromoCode>(promo => promo.UsedCount == 1)),
            Times.Once);

        fixture.Sms.Verify(
            sender => sender.Send(
                "01000000000",
                "Booking DN-TEST cancelled"),
            Times.Once);

        fixture.Email.Verify(
            sender => sender.Send(
                "customer@example.com",
                "Booking DN-TEST cancelled",
                "Booking DN-TEST cancelled"),
            Times.Once);
    }

    [Fact]
    public void Cancel_BookingDoesNotExist_ThrowsBookingNotFoundException()
    {
        CancellationFixture fixture = new();
        fixture.BookingExists = false;

        Assert.Throws<BookingNotFoundException>(
            () => fixture.Sut.Cancel(1));
    }

    [Fact]
    public void Cancel_BookingIsNotReserved_ThrowsInvalidBookingStateException()
    {
        CancellationFixture fixture = new();
        fixture.Booking.Status = BookingStatus.Active;

        Assert.Throws<InvalidBookingStateException>(
            () => fixture.Sut.Cancel(1));
    }

    [Fact]
    public void Cancel_PickupTimeReached_ThrowsInvalidBookingStateException()
    {
        CancellationFixture fixture = new();
        fixture.SetNow(Pickup);

        InvalidBookingStateException exception =
            Assert.Throws<InvalidBookingStateException>(
                () => fixture.Sut.Cancel(1));

        Assert.Equal("Booking has already started", exception.Message);
    }
    
    
    [Theory]
    [InlineData(1500, 500, 0)]
    [InlineData(1000, 0, 0)]
    [InlineData(500, 0, 500)]
    [Trait("Rule", "BR-52")]
    public void Cancel_DepositComparedToFee_CalculatesRefundAndAdditionalCharge(
        decimal deposit,
        decimal expectedRefund,
        decimal expectedCharge)
    {
        CancellationFixture fixture = new();
        fixture.Booking.Deposit = deposit;

        // Under 24 hours: cancellation fee = 1000
        fixture.SetNow(Pickup.AddHours(-23));

        decimal fee = fixture.Sut.Cancel(1);

        Assert.Equal(1000m, fee);

        if (expectedRefund > 0)
        {
            fixture.Payments.Verify(
                payment => payment.Refund(
                    1,
                    expectedRefund,
                    "Deposit refund DN-TEST"),
                Times.Once);
        }
        else
        {
            fixture.Payments.Verify(
                payment => payment.Refund(
                    It.IsAny<int>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        if (expectedCharge > 0)
        {
            fixture.Payments.Verify(
                payment => payment.Charge(
                    1,
                    expectedCharge,
                    "Cancellation DN-TEST"),
                Times.Once);
        }
        else
        {
            fixture.Payments.Verify(
                payment => payment.Charge(
                    It.IsAny<int>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>()),
                Times.Never);
        }
    }

    private sealed class CancellationFixture
    {
        public readonly Mock<IBookingRepository> Bookings = new();
        public readonly Mock<IVehicleRepository> Vehicles = new();
        public readonly Mock<ICustomerRepository> Customers = new();
        public readonly Mock<IPromoCodeRepository> Promos = new();
        public readonly Mock<IPaymentGateway> Payments = new();
        public readonly Mock<ISmsSender> Sms = new();
        public readonly Mock<IEmailSender> Email = new();
        public readonly Mock<IAuditLog> Audit = new();

        public bool BookingExists { get; set; } = true;

        public readonly Booking Booking = new()
        {
            Id = 1,
            Reference = "DN-TEST",
            CustomerId = 1,
            VehicleId = 1,
            PickupAt = Pickup,
            Status = BookingStatus.Reserved,
            Deposit = 1000m
        };

        public readonly Vehicle Vehicle = new()
        {
            Id = 1,
            DailyRate = 1000m
        };

        public readonly Customer Customer = new()
        {
            Id = 1,
            Email = "customer@example.com",
            Phone = "01000000000",
            LoyaltyTier = LoyaltyTier.None
        };

        private DateTime Now { get; set; } = Pickup.AddHours(-72);

        public CancellationService Sut
        {
            get
            {
                DateTimeOffset localTime = new(
                    DateTime.SpecifyKind(Now, DateTimeKind.Unspecified),
                    TimeZoneInfo.Local.GetUtcOffset(Now));

                TimeProvider clock = new FixedTimeProvider(
                    localTime.ToUniversalTime());

                Bookings.Setup(repository => repository.GetById(1))
                    .Returns(() => BookingExists ? Booking : null);

                Vehicles.Setup(repository => repository.GetById(1))
                    .Returns(Vehicle);

                Customers.Setup(repository => repository.GetById(1))
                    .Returns(Customer);

                return new CancellationService(
                    Bookings.Object,
                    Vehicles.Object,
                    Customers.Object,
                    Promos.Object,
                    Payments.Object,
                    Sms.Object,
                    Email.Object,
                    Audit.Object,
                    clock);
            }
        }

        public void SetNow(DateTime now) => Now = now;
    }
}
