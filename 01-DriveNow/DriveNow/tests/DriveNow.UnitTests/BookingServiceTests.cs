using AutoFixture;
using AutoFixture.Xunit2;
using DriveNow.Application.Abstractions;
using DriveNow.Application.Repositories;
using DriveNow.Application.Services;
using DriveNow.Application.Models;
using DriveNow.Domain.Calculations;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.UnitTests;
using DriveNow.UnitTests.Customizations;
using Moq;

public class BookingServiceTests
{
    
    
    [Fact]
    [Trait("Rule", "BR-10")]
    public void CreateBooking_CustomerDoesNotExist_ThrowsCustomerNotFoundException()
    {
        // Arrange
        StubCustomerRepository customerRepository = new StubCustomerRepository();

        BookingService sut = new BookingService(
            customerRepository,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            TimeProvider.System);
        
        BookingRequest request = new BookingRequest
        {
            CustomerId = 999,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
        };

        // Act
        Action act = () => sut.CreateBooking(request);

        // Assert
        CustomerNotFoundException exception =
            Assert.Throws<CustomerNotFoundException>(act);

        Assert.Equal("Customer #999 not found", exception.Message);
    }
    
    [Fact]
    [Trait("Rule", "BR-11")]
    public void CreateBooking_BlacklistedCustomer_ThrowsBookingRejectedException()
    {
        // Arrange
        BookingServiceTestFixture fixture = new BookingServiceTestFixture();

        Customer customer = new Customer
        {
            Id = 1,
            IsBlacklisted = true
        };

        fixture.Customers.Data.Add(customer.Id, customer);

        BookingService sut = fixture.CreateSut();

        BookingRequest request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
        };

        // Act
        Action act = () => sut.CreateBooking(request);

        // Assert
        BookingRejectedException exception =
            Assert.Throws<BookingRejectedException>(act);

        Assert.Equal("Customer is blacklisted", exception.Message);
    }
    
    
    [Fact]
    [Trait("Rule", "BR-12")]
    public void CreateBooking_CustomerYoungerThan21_ThrowsBookingRejectedException()
    {
        // Arrange
        BookingServiceTestFixture fixture = new BookingServiceTestFixture();

        Customer customer = new Customer
        {
            Id = 1,
            DateOfBirth = new DateTime(2005, 11, 11),
            LicenseIssueDate = new DateTime(2020, 1, 1),
            IsBlacklisted = false
        };

        fixture.Customers.Data.Add(customer.Id, customer);

        BookingService sut = fixture.CreateSut();

        BookingRequest request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
        };

        // Act
        Action act = () => sut.CreateBooking(request);

        // Assert
        BookingRejectedException exception =
            Assert.Throws<BookingRejectedException>(act);

        Assert.Equal("Driver must be at least 21", exception.Message);
    }
    
    
    [Fact]
    [Trait("Rule", "BR-13")]
    public void CreateBooking_LicenseHeldForLessThanOneYear_ThrowsBookingRejectedException()
    {
        // Arrange
        BookingServiceTestFixture fixture = new BookingServiceTestFixture();

        Customer customer = new Customer
        {
            Id = 1,
            DateOfBirth = new DateTime(1996, 1, 1),
            LicenseIssueDate = new DateTime(2026, 1, 1),
            IsBlacklisted = false
        };

        fixture.Customers.Data.Add(customer.Id, customer);

        BookingService sut = fixture.CreateSut();

        BookingRequest request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
        };

        // Act
        Action act = () => sut.CreateBooking(request);

        // Assert
        BookingRejectedException exception =
            Assert.Throws<BookingRejectedException>(act);

        Assert.Equal("License must be held for at least 1 year", exception.Message);
    }
    
    
    [Fact]
    [Trait("Rule", "BR-14")]
    public void CreateBooking_VehicleDoesNotExist_ThrowsVehicleNotFoundException()
    {
        // Arrange
        BookingServiceTestFixture fixture = new BookingServiceTestFixture();

        Customer customer = new Customer
        {
            Id = 1,
            DateOfBirth = new DateTime(1996, 1, 1),
            LicenseIssueDate = new DateTime(2020, 1, 1),
            IsBlacklisted = false
        };

        fixture.Customers.Data.Add(customer.Id, customer);

        BookingService sut = fixture.CreateSut();

        BookingRequest request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = 999,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
        };

        // Act
        Action act = () => sut.CreateBooking(request);

        // Assert
        VehicleNotFoundException exception =
            Assert.Throws<VehicleNotFoundException>(act);

        Assert.Equal("Vehicle #999 not found", exception.Message);
    }
    
    [Fact]
    [Trait("Rule", "BR-15")]
    public void CreateBooking_VehicleIsNotAvailable_ThrowsBookingRejectedException()
{
    BookingServiceTestFixture fixture = new();

    fixture.Customers.Data[1] = new Customer
    {
        Id = 1,
        DateOfBirth = new DateTime(1996, 1, 1),
        LicenseIssueDate = new DateTime(2020, 1, 1),
        IsBlacklisted = false
    };

    fixture.Vehicles.Data[1] = new Vehicle
    {
        Id = 1,
        Category = VehicleCategory.Economy,
        IsAvailable = false
    };

    BookingService sut = fixture.CreateSut();

    BookingRequest request = new()
    {
        CustomerId = 1,
        VehicleId = 1,
        PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
        ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
    };

    BookingRejectedException exception =
        Assert.Throws<BookingRejectedException>(() => sut.CreateBooking(request));

    Assert.Equal("Vehicle is not available", exception.Message);
}

    
    [Fact]
    [Trait("Rule", "BR-16")]
    public void CreateBooking_LuxuryVehicleAndMinimumEligibilityRequirements_AllowsBooking()
{
    BookingServiceTestFixture fixture = new();

    fixture.Customers.Data[1] = new Customer
    {
        Id = 1,
        DateOfBirth = new DateTime(2001, 11, 10),
        LicenseIssueDate = new DateTime(2023, 11, 10),
        IsBlacklisted = false
    };

    fixture.Vehicles.Data[1] = new Vehicle
    {
        Id = 1,
        Category = VehicleCategory.Luxury,
        IsAvailable = true
    };

    BookingService sut = fixture.CreateSut();

    BookingRequest request = new()
    {
        CustomerId = 1,
        VehicleId = 1,
        PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
        ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
    };

    Booking result = sut.CreateBooking(request);

    Assert.NotNull(result);
    Assert.Equal(BookingStatus.Reserved, result.Status);
}

    [Fact]
    [Trait("Rule", "BR-17")]
    public void CreateBooking_VanAndLicenseHeldForExactlyTwoYears_AllowsBooking()
{
    BookingServiceTestFixture fixture = new();

    fixture.Customers.Data[1] = new Customer
    {
        Id = 1,
        DateOfBirth = new DateTime(1996, 1, 1),
        LicenseIssueDate = new DateTime(2024, 11, 10),
        IsBlacklisted = false
    };

    fixture.Vehicles.Data[1] = new Vehicle
    {
        Id = 1,
        Category = VehicleCategory.Van,
        IsAvailable = true
    };

    BookingService sut = fixture.CreateSut();

    BookingRequest request = new()
    {
        CustomerId = 1,
        VehicleId = 1,
        PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
        ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
    };

    Booking result = sut.CreateBooking(request);

    Assert.NotNull(result);
    Assert.Equal(BookingStatus.Reserved, result.Status);
}

    [Fact]
    [Trait("Rule", "BR-18")]
    public void CreateBooking_PickupExactlyTwoHoursFromNow_AllowsBooking()
    {
        BookingServiceTestFixture fixture = new();

        DateTime localNow = new(2026, 11, 3, 10, 0, 0);
        TimeSpan localOffset = TimeZoneInfo.Local.GetUtcOffset(localNow);
        DateTimeOffset now = new(localNow, localOffset);

        fixture.TimeProvider = new FixedTimeProvider(now);

        DateTime providerNow = fixture.TimeProvider.GetLocalNow().DateTime;

        Customer customer = CreateValidCustomer();
        Vehicle vehicle = CreateValidVehicle();

        fixture.Customers.Data[customer.Id] = customer;
        fixture.Vehicles.Data[vehicle.Id] = vehicle;

        BookingService sut = fixture.CreateSut();

        BookingRequest request = new()
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = providerNow.AddHours(2),
            ReturnAt = providerNow.AddDays(2)
        };

        Booking result = sut.CreateBooking(request);

        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Reserved, result.Status);
    }

    [Fact]
    [Trait("Rule", "BR-17")]
    public void CreateBooking_VanAndLicenseHeldForLessThan2Years_ThrowsBookingRejectedException()
{
    BookingServiceTestFixture fixture = new();

    fixture.Customers.Data[1] = new Customer
    {
        Id = 1,
        DateOfBirth = new DateTime(1996, 1, 1),
        LicenseIssueDate = new DateTime(2025, 1, 1),
        IsBlacklisted = false
    };

    fixture.Vehicles.Data[1] = new Vehicle
    {
        Id = 1,
        Category = VehicleCategory.Van,
        IsAvailable = true
    };

    BookingService sut = fixture.CreateSut();

    BookingRequest request = new()
    {
        CustomerId = 1,
        VehicleId = 1,
        PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
        ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
    };

    BookingRejectedException exception =
        Assert.Throws<BookingRejectedException>(() => sut.CreateBooking(request));

    Assert.Equal("Vans require 2 years of license", exception.Message);
}
    
    [Fact]
    [Trait("Rule", "BR-18")]
    public void CreateBooking_PickupLessThanTwoHoursFromNow_ThrowsBookingRejectedException()
{
    BookingServiceTestFixture fixture = new();

    fixture.Customers.Data[1] = new Customer
    {
        Id = 1,
        DateOfBirth = new DateTime(1996, 1, 1),
        LicenseIssueDate = new DateTime(2020, 1, 1),
        IsBlacklisted = false
    };

    fixture.Vehicles.Data[1] = new Vehicle
    {
        Id = 1,
        Category = VehicleCategory.Economy,
        IsAvailable = true
    };

    BookingService sut = fixture.CreateSut();

    DateTime now = DateTime.Now;

    BookingRequest request = new()
    {
        CustomerId = 1,
        VehicleId = 1,
        PickupAt = now.AddHours(1),
        ReturnAt = now.AddDays(2)
    };

    BookingRejectedException exception =
        Assert.Throws<BookingRejectedException>(() => sut.CreateBooking(request));

    Assert.Equal("Pickup must be at least 2 hours from now", exception.Message);
}

    [Fact]
    [Trait("Rule", "BR-19")]
    public void CreateBooking_RentalPeriodExceedsThirtyDays_ThrowsBookingRejectedException()
{
    BookingServiceTestFixture fixture = new();

    fixture.Customers.Data[1] = new Customer
    {
        Id = 1,
        DateOfBirth = new DateTime(1996, 1, 1),
        LicenseIssueDate = new DateTime(2020, 1, 1),
        IsBlacklisted = false
    };

    fixture.Vehicles.Data[1] = new Vehicle
    {
        Id = 1,
        Category = VehicleCategory.Economy,
        IsAvailable = true
    };

    BookingService sut = fixture.CreateSut();

    DateTime now = DateTime.Now;

    BookingRequest request = new()
    {
        CustomerId = 1,
        VehicleId = 1,
        PickupAt = now.AddHours(3),
        ReturnAt = now.AddDays(31)
    };

    BookingRejectedException exception =
        Assert.Throws<BookingRejectedException>(() => sut.CreateBooking(request));

    Assert.Equal("Rental period is invalid", exception.Message);
}

    [Fact]
    [Trait("Rule", "BR-20")]
    public void CreateBooking_MoreThanThreeAdditionalDrivers_ThrowsBookingRejectedException()
{
    BookingServiceTestFixture fixture = new();

    fixture.Customers.Data[1] = new Customer
    {
        Id = 1,
        DateOfBirth = new DateTime(1996, 1, 1),
        LicenseIssueDate = new DateTime(2020, 1, 1),
        IsBlacklisted = false
    };

    fixture.Vehicles.Data[1] = new Vehicle
    {
        Id = 1,
        Category = VehicleCategory.Economy,
        IsAvailable = true
    };

    BookingService sut = fixture.CreateSut();

    DateTime now = DateTime.Now;

    BookingRequest request = new()
    {
        CustomerId = 1,
        VehicleId = 1,
        PickupAt = now.AddHours(3),
        ReturnAt = now.AddDays(2),
        AdditionalDrivers = 4
    };

    BookingRejectedException exception =
        Assert.Throws<BookingRejectedException>(() => sut.CreateBooking(request));

    Assert.Equal("Up to 3 additional drivers allowed", exception.Message);
}
    
    [Fact]
    [Trait("Rule", "BR-21")]
    public void CreateBooking_OverlappingBookingExists_ThrowsBookingRejectedException()
    {
        BookingServiceTestFixture fixture = new();

        fixture.Customers.Data[1] = new Customer
        {
            Id = 1,
            DateOfBirth = new DateTime(1996, 1, 1),
            LicenseIssueDate = new DateTime(2020, 1, 1),
            IsBlacklisted = false
        };

        fixture.Vehicles.Data[1] = new Vehicle
        {
            Id = 1,
            Category = VehicleCategory.Economy,
            IsAvailable = true
        };

        fixture.Bookings.Data.Add(new Booking
        {
            Id = 10,
            CustomerId = 2,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0),
            Status = BookingStatus.Reserved
        });

        BookingService sut = fixture.CreateSut();

        BookingRequest request = new()
        {
            CustomerId = 1,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 11, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 13, 10, 0, 0)
        };

        BookingRejectedException exception =
            Assert.Throws<BookingRejectedException>(() => sut.CreateBooking(request));

        Assert.Equal("Vehicle is already booked for these dates", exception.Message);
    }
    
    
    [Fact]
    [Trait("Rule", "BR-21")]
    public void CreateBooking_StartsWhenPreviousBookingEnds_AllowsBooking()
    {
        BookingServiceTestFixture fixture = new();

        fixture.Customers.Data[1] = new Customer
        {
            Id = 1,
            DateOfBirth = new DateTime(1996, 1, 1),
            LicenseIssueDate = new DateTime(2020, 1, 1),
            IsBlacklisted = false
        };

        fixture.Vehicles.Data[1] = new Vehicle
        {
            Id = 1,
            Category = VehicleCategory.Economy,
            IsAvailable = true
        };

        fixture.Bookings.Data.Add(new Booking
        {
            CustomerId = 2,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0),
            Status = BookingStatus.Reserved
        });

        BookingService sut = fixture.CreateSut();

        BookingRequest request = new()
        {
            CustomerId = 1,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 12, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 14, 10, 0, 0)
        };

        Exception? exception = Record.Exception(
            () => sut.CreateBooking(request));

        Assert.Null(exception);
    }
    
    [Fact]
    [Trait("Rule", "BR-22")]
    public void CreateBooking_CustomerHasTwoOpenBookings_ThrowsBookingRejectedException()
    {
        BookingServiceTestFixture fixture = new();
    
        fixture.Customers.Data[1] = new Customer
        {
            Id = 1,
            DateOfBirth = new DateTime(1996, 1, 1),
            LicenseIssueDate = new DateTime(2020, 1, 1),
            IsBlacklisted = false
        };
    
        fixture.Vehicles.Data[1] = new Vehicle
        {
            Id = 1,
            Category = VehicleCategory.Economy,
            IsAvailable = true
        };
    
        fixture.Bookings.Data.AddRange(
            new Booking
            {
                CustomerId = 1,
                VehicleId = 2,
                Status = BookingStatus.Reserved
            },
            new Booking
            {
                CustomerId = 1,
                VehicleId = 3,
                Status = BookingStatus.Active
            });
    
        BookingService sut = fixture.CreateSut();
    
        BookingRequest request = new()
        {
            CustomerId = 1,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
        };
    
        BookingRejectedException exception =
            Assert.Throws<BookingRejectedException>(() => sut.CreateBooking(request));
    
        Assert.Equal("Customer already has 2 active bookings", exception.Message);
    }
    
    [Fact]
    [Trait("Rule", "BR-23")]
    public void CreateBooking_ValidRequest_CalculatesPriceUsingPricingService()
    {
        // Arrange
        BookingServiceTestFixture fixture = new();

        Customer customer = new()
        {
            Id = 1,
            DateOfBirth = new DateTime(1990, 1, 1),
            LicenseIssueDate = new DateTime(2015, 1, 1),
            LoyaltyTier = LoyaltyTier.None,
            IsBlacklisted = false,
            Phone = "01000000000",
            Email = "test@example.com"
        };

        Vehicle vehicle = new()
        {
            Id = 1,
            Category = VehicleCategory.Economy,
            DailyRate = 1000m,
            Mileage = 50000,
            IsAvailable = true
        };

        fixture.Customers.Data[customer.Id] = customer;
        fixture.Vehicles.Data[vehicle.Id] = vehicle;

        BookingService sut = fixture.CreateSut();

        BookingRequest request = new()
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0),
            AdditionalDrivers = 0,
            PromoCode = null
        };

        // Act
        Booking result = sut.CreateBooking(request);

        // Assert
        Assert.NotNull(result.Price);
        Assert.Equal(2, result.Price.RentalDays);
        Assert.Equal(2000m, result.Price.BaseAmount);
        Assert.Equal(0m, result.Price.WeekendSurcharge);
        Assert.Equal(0m, result.Price.YoungDriverSurcharge);
        Assert.Equal(0m, result.Price.DurationDiscount);
        Assert.Equal(0m, result.Price.LoyaltyDiscount);
        Assert.Equal(0m, result.Price.PromoDiscount);
        Assert.Equal(0m, result.Price.InsuranceCost);
        Assert.Equal(0m, result.Price.AdditionalDriversCost);
        Assert.Equal(2000m, result.Price.Total);
    }
    
    
    [Fact]
    [Trait("Rule", "BR-24")]
    [Trait("Rule", "BR-25")]
    public void CreateBooking_ValidRequest_SetsBookingPropertiesAndReference()
    {
        // Arrange
        BookingServiceTestFixture fixture = new();

        Customer customer = CreateValidCustomer();
        Vehicle vehicle = CreateValidVehicle();

        fixture.Customers.Data[customer.Id] = customer;
        fixture.Vehicles.Data[vehicle.Id] = vehicle;

        BookingService sut = fixture.CreateSut();

        BookingRequest request = CreateValidRequest();

        // Act
        Booking result = sut.CreateBooking(request);

        // Assert
        Assert.Equal(BookingStatus.Reserved, result.Status);
        Assert.True(result.CreatedAt <= DateTime.Now);
        Assert.True(result.CreatedAt >= DateTime.Now.AddSeconds(-5));
        Assert.Equal(vehicle.Mileage, result.StartMileage);

        Assert.Equal("DN-20261110-AB12", result.Reference);
    }
    
    [Theory]
    [InlineData(VehicleCategory.Economy, InsurancePlan.Basic, LoyaltyTier.None, 1000)]
    [InlineData(VehicleCategory.Economy, InsurancePlan.Premium, LoyaltyTier.None, 500)]
    [InlineData(VehicleCategory.SUV, InsurancePlan.Basic, LoyaltyTier.None, 2500)]
    [InlineData(VehicleCategory.SUV, InsurancePlan.Premium, LoyaltyTier.None, 1250)]
    [InlineData(VehicleCategory.Luxury, InsurancePlan.Basic, LoyaltyTier.None, 5000)]
    [InlineData(VehicleCategory.Luxury, InsurancePlan.Premium, LoyaltyTier.None, 2500)]
    [InlineData(VehicleCategory.Economy, InsurancePlan.Basic, LoyaltyTier.Platinum, 0)]
    [Trait("Rule", "BR-26")]
    public void CreateBooking_ValidRequest_ChargesCorrectDeposit(
        VehicleCategory category,
        InsurancePlan insurance,
        LoyaltyTier tier,
        decimal expectedDeposit)
    {
        // Arrange
        BookingServiceTestFixture fixture = new();

        Customer customer = CreateValidCustomer();
        customer.LoyaltyTier = tier;

        Vehicle vehicle = CreateValidVehicle();
        vehicle.Category = category;

        fixture.Customers.Data[customer.Id] = customer;
        fixture.Vehicles.Data[vehicle.Id] = vehicle;

        BookingService sut = fixture.CreateSut();

        BookingRequest request = CreateValidRequest();
        request.InsurancePlan = insurance;

        // Act
        Booking result = sut.CreateBooking(request);

        // Assert
        Assert.Equal(expectedDeposit, result.Deposit);

        if (tier == LoyaltyTier.Platinum)
        {
            fixture.PaymentGateway.Verify(
                payment => payment.Charge(
                    It.IsAny<int>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        else
        {
            fixture.PaymentGateway.Verify(
                payment => payment.Charge(
                    customer.Id,
                    expectedDeposit,
                    $"Deposit {result.Reference}"),
                Times.Once);
        }
    }
    
    [Fact]
    [Trait("Rule", "BR-27")]
    public void CreateBooking_ValidRequest_UpdatesPromoSavesBookingAndSendsNotifications()
    {
        // Arrange
        BookingServiceTestFixture fixture = new();

        Customer customer = CreateValidCustomer();
        Vehicle vehicle = CreateValidVehicle();

        PromoCode promo = new()
        {
            Code = "SAVE10",
            DiscountPercent = 10,
            ValidFrom = new DateTime(2026, 1, 1),
            ValidUntil = new DateTime(2026, 12, 31),
            MinDays = 1,
            MaxUses = 100,
            UsedCount = 5,
            AllowedCategories = new List<VehicleCategory>
            {
                VehicleCategory.Economy
            }
        };

        fixture.Customers.Data[customer.Id] = customer;
        fixture.Vehicles.Data[vehicle.Id] = vehicle;
        fixture.Promos.Data[promo.Code] = promo;

        BookingService sut = fixture.CreateSut();

        BookingRequest request = CreateValidRequest();
        request.PromoCode = promo.Code;

        // Act
        Booking result = sut.CreateBooking(request);

        // Assert
        Assert.Equal(6, promo.UsedCount);
        Assert.Single(fixture.Promos.UpdatedPromos);

        Assert.Contains(result, fixture.Bookings.Data);

        string expectedMessage = $"Booking {result.Reference} confirmed";

        Assert.Contains(
            fixture.Sms.SentMessages,
            message => message.Phone == customer.Phone &&
                       message.Text == expectedMessage);

        Assert.Contains(
            fixture.Email.SentEmails,
            email => email.To == customer.Email &&
                     email.Subject == expectedMessage &&
                     email.Body == expectedMessage);

        Assert.Contains(
            $"Booking {result.Reference} created",
            fixture.AuditLog.Messages);
    }
    
    [Fact]
    public void CreateBooking_CustomerDoesNotExist_DoesNotGenerateReference()
    {
        BookingServiceTestFixture fixture = new();

        DummyReferenceGenerator dummyReferenceGenerator =
            new DummyReferenceGenerator();

        BookingService sut = new BookingService(
            fixture.Customers,
            fixture.Vehicles,
            fixture.Bookings,
            fixture.Promos,
            new PricingService(fixture.Promos, TimeProvider.System),
            fixture.PaymentGateway.Object,
            fixture.Sms,
            fixture.Email,
            fixture.AuditLog,
            dummyReferenceGenerator,
            TimeProvider.System);

        BookingRequest request = CreateValidRequest();

        Assert.Throws<CustomerNotFoundException>(
            () => sut.CreateBooking(request));
    }
    
    [Fact]
    public void CreateBooking_WithDeposit_ChargesExpectedAmount()
    {
        BookingServiceTestFixture fixture = new();

        Customer customer = CreateValidCustomer();
        Vehicle vehicle = CreateValidVehicle();

        fixture.Customers.Data.Add(customer.Id, customer);
        fixture.Vehicles.Data.Add(vehicle.Id, vehicle);

        BookingRequest request = CreateValidRequest();

        HandWrittenMockPaymentGateway mockPaymentGateway =
            new HandWrittenMockPaymentGateway
            {
                ExpectedCustomerId = customer.Id,
                ExpectedAmount = 1000m,
                ExpectedDescription = $"Deposit DN-{request.PickupAt:yyyyMMdd}-AB12"
            };
        
        BookingService sut = fixture.CreateSut(paymentGateway: mockPaymentGateway);

        Booking booking = sut.CreateBooking(request);

        mockPaymentGateway.Verify();

        Assert.Equal(1000m, booking.Deposit);
    }
    
    [Fact]
    [Trait("Rule", "BR-10")]
    public void CreateBooking_CustomerDoesNotExist_WithMoq_ThrowsCustomerNotFoundException()
    {
        BookingServiceTestFixture fixture = new();

        Mock<ICustomerRepository> customerRepository = new();

        customerRepository
            .Setup(repository => repository.GetById(999))
            .Returns((Customer?)null);

        BookingService sut = fixture.CreateSut(
            customerRepository: customerRepository.Object);

        BookingRequest request = CreateValidRequest();
        request.CustomerId = 999;

        CustomerNotFoundException exception =
            Assert.Throws<CustomerNotFoundException>(
                () => sut.CreateBooking(request));

        Assert.Equal("Customer #999 not found", exception.Message);

        customerRepository.Verify(
            repository => repository.GetById(999),
            Times.Once);
    }
    
    [Fact]
    [Trait("Rule", "BR-14")]
    public void CreateBooking_VehicleDoesNotExist_WithMoq_ThrowsVehicleNotFoundException()
    {
        BookingServiceTestFixture fixture = new();

        Mock<IVehicleRepository> vehicleRepository = new();

        vehicleRepository
            .Setup(repository => repository.GetById(999))
            .Returns((Vehicle?)null);

        Customer customer = CreateValidCustomer();
        fixture.Customers.Data[customer.Id] = customer;

        BookingService sut = fixture.CreateSut(
            vehicleRepository: vehicleRepository.Object);

        BookingRequest request = CreateValidRequest();
        request.VehicleId = 999;

        VehicleNotFoundException exception =
            Assert.Throws<VehicleNotFoundException>(
                () => sut.CreateBooking(request));

        Assert.Equal("Vehicle #999 not found", exception.Message);

        vehicleRepository.Verify(
            repository => repository.GetById(999),
            Times.Once);
    }
    
    [Theory]
    [AutoMoqData]
    public void CreateBooking_CustomerDoesNotExist_WithAutoMoq_ThrowsCustomerNotFoundException(
        [Frozen] Mock<ICustomerRepository> customerRepository,
        BookingService sut)
    {
        customerRepository
            .Setup(repository => repository.GetById(999))
            .Returns((Customer?)null);
    
        BookingRequest request = CreateValidRequest();
        request.CustomerId = 999;
    
        CustomerNotFoundException exception =
            Assert.Throws<CustomerNotFoundException>(
                () => sut.CreateBooking(request));
    
        Assert.Equal("Customer #999 not found", exception.Message);
    
        customerRepository.Verify(
            repository => repository.GetById(999),
            Times.Once);
    }
    
    [Fact]
    [Trait("Requirement", "R4")]
    public void AutoFixture_AnonymousCustomer_MayViolateBusinessRules()
    {
        Fixture fixture = new();

        Customer customer = fixture.Create<Customer>();

        int age = RentalMath.CalculateAge(
            customer.DateOfBirth,
            new DateTime(2026, 11, 10));

        int licenseYears = RentalMath.CalculateLicenseYears(
            customer.LicenseIssueDate,
            new DateTime(2026, 11, 10));

        bool isValid =
            age >= 21 &&
            licenseYears >= 1 &&
            !customer.IsBlacklisted;

        Assert.False(isValid);
    }
    
    [Fact]
    [Trait("Requirement", "R4")]
    public void ValidCustomerCustomization_CreatesCustomerThatMeetsBusinessRules()
    {
        Fixture fixture = new();
        fixture.Customize(new ValidCustomerCustomization());

        Customer customer = fixture.Create<Customer>();

        int age = RentalMath.CalculateAge(
            customer.DateOfBirth,
            new DateTime(2026, 11, 10));

        int licenseYears = RentalMath.CalculateLicenseYears(
            customer.LicenseIssueDate,
            new DateTime(2026, 11, 10));

        Assert.True(age >= 21);
        Assert.True(licenseYears >= 1);
        Assert.False(customer.IsBlacklisted);
    }
    
    [Theory]
    [InlineAutoData(20)]
    [InlineAutoData(21)]
    [InlineAutoData(25)]
    [Trait("Requirement", "R5")]
    public void CreateBooking_CustomerAgeBoundary_EnforcesMinimumAge(
        int age,
        BookingRequest request)
    {
        BookingServiceTestFixture fixture = new();

        DateTime pickupAt = new(2026, 11, 10, 10, 0, 0);

        Customer customer = CreateValidCustomer();
        customer.DateOfBirth = pickupAt.AddYears(-age);
        customer.LicenseIssueDate = pickupAt.AddYears(-5);

        Vehicle vehicle = CreateValidVehicle();

        fixture.Customers.Data.Add(customer.Id, customer);
        fixture.Vehicles.Data.Add(vehicle.Id, vehicle);

        request.CustomerId = customer.Id;
        request.VehicleId = vehicle.Id;
        request.PickupAt = pickupAt;
        request.ReturnAt = pickupAt.AddDays(2);
        request.AdditionalDrivers = 0;
        request.PromoCode = null;
        request.InsurancePlan = InsurancePlan.Basic;

        BookingService sut = fixture.CreateSut();

        if (age == 20)
        {
            BookingRejectedException exception =
                Assert.Throws<BookingRejectedException>(
                    () => sut.CreateBooking(request));

            Assert.Equal("Driver must be at least 21", exception.Message);
        }
        else
        {
            Booking booking = sut.CreateBooking(request);

            Assert.NotNull(booking);
        }
    }
    
    
    [Fact]
    [Trait("Requirement", "R7")]
    public Task CreateBooking_ValidRequest_MatchesSnapshot()
    {
        BookingServiceTestFixture fixture = new();

        fixture.TimeProvider = new FixedTimeProvider(
            new DateTimeOffset(2026, 11, 3, 10, 0, 0, TimeSpan.Zero));

        Customer customer = CreateValidCustomer();
        Vehicle vehicle = CreateValidVehicle();

        fixture.Customers.Data[customer.Id] = customer;
        fixture.Vehicles.Data[vehicle.Id] = vehicle;

        BookingService sut = fixture.CreateSut();

        BookingRequest request = CreateValidRequest();

        Booking booking = sut.CreateBooking(request);

        return Verify(booking);
    }
    
    [Fact]
    [Trait("Rule", "BR-19")]
    public void CreateBooking_RentalPeriodIsExactlyThirtyDays_AllowsBooking()
    {
        BookingServiceTestFixture fixture = new();

        DateTime localNow = new(2026, 11, 3, 10, 0, 0);
        TimeSpan localOffset = TimeZoneInfo.Local.GetUtcOffset(localNow);
        DateTimeOffset now = new(localNow, localOffset);

        fixture.TimeProvider = new FixedTimeProvider(now);

        Customer customer = CreateValidCustomer();
        Vehicle vehicle = CreateValidVehicle();

        fixture.Customers.Data[customer.Id] = customer;
        fixture.Vehicles.Data[vehicle.Id] = vehicle;

        BookingService sut = fixture.CreateSut();

        DateTime pickupAt = fixture.TimeProvider.GetLocalNow().DateTime.AddHours(3);

        BookingRequest request = new()
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(30)
        };

        Booking result = sut.CreateBooking(request);

        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Reserved, result.Status);
    }
    
    [Fact]
    [Trait("Rule", "BR-22")]
    public void CreateBooking_CustomerHasOneOpenBooking_AllowsAnotherBooking()
    {
        BookingServiceTestFixture fixture = new();

        Customer customer = CreateValidCustomer();
        Vehicle vehicle = CreateValidVehicle();

        fixture.Customers.Data[customer.Id] = customer;
        fixture.Vehicles.Data[vehicle.Id] = vehicle;

        fixture.Bookings.Data.Add(new Booking
        {
            Id = 10,
            CustomerId = customer.Id,
            VehicleId = 2,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0),
            Status = BookingStatus.Reserved
        });

        BookingService sut = fixture.CreateSut();

        BookingRequest request = CreateValidRequest();

        Booking result = sut.CreateBooking(request);

        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Reserved, result.Status);
    }
    
    [Theory]
    [InlineData(-61, false)]
    [InlineData(-60, true)]
    [InlineData(-59, true)]
    [Trait("Rule", "BR-28")]
    public void StartRental_PickupTimeBoundary_EnforcesOneHourRule(int minutesBeforePickup, bool shouldStart)
    {
        BookingServiceTestFixture fixture = new();

        DateTime pickupAt = new(2026, 11, 10, 10, 0, 0);
        DateTime now = pickupAt.AddMinutes(minutesBeforePickup);

        DateTimeOffset localTime = new(DateTime.SpecifyKind(now, DateTimeKind.Unspecified), TimeZoneInfo.Local.GetUtcOffset(now));

        fixture.TimeProvider = new FixedTimeProvider(localTime.ToUniversalTime());

        Booking booking = new()
        {
            Id = 1,
            Reference = "DN-TEST",
            PickupAt = pickupAt,
            Status = BookingStatus.Reserved
        };

        fixture.Bookings.Data.Add(booking);

        BookingService sut = fixture.CreateSut();

        if (shouldStart)
        {
            sut.StartRental(1);
            Assert.Equal(BookingStatus.Active, booking.Status);
        }
        else
        {
            InvalidBookingStateException exception =
                Assert.Throws<InvalidBookingStateException>(
                    () => sut.StartRental(1));

            Assert.Equal("Too early to start this rental", exception.Message);
            Assert.Equal(BookingStatus.Reserved, booking.Status);
        }
    }
    
    
    
    
    
    
    
    
    private static Customer CreateValidCustomer()
    {
        return new Customer
        {
            Id = 1,
            DateOfBirth = new DateTime(1990, 1, 1),
            LicenseIssueDate = new DateTime(2015, 1, 1),
            LoyaltyTier = LoyaltyTier.None,
            IsBlacklisted = false,
            Phone = "01000000000",
            Email = "test@example.com"
        };
    }
    
    private static Vehicle CreateValidVehicle()
    {
        return new Vehicle
        {
            Id = 1,
            Category = VehicleCategory.Economy,
            DailyRate = 1000m,
            Mileage = 50000,
            IsAvailable = true
        };
    }
    
    private static BookingRequest CreateValidRequest()
    {
        return new BookingRequest
        {
            CustomerId = 1,
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0,
            PromoCode = null
        };
    }
}



public class BookingServiceTestFixture
{
    public StubCustomerRepository Customers { get; } = new();
    public StubVehicleRepository Vehicles { get; } = new();
    public StubBookingRepository Bookings { get; } = new();

    public SpyPromoCodeRepository Promos { get; } = new();
    public SpySmsSender Sms { get; } = new();
    public SpyEmailSender Email { get; } = new();
    public SpyAuditLog AuditLog { get; } = new();

    public Mock<IPaymentGateway> PaymentGateway { get; } = new();

    public StubReferenceGenerator ReferenceGenerator { get; } = new();

    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    public BookingService CreateSut(
        ICustomerRepository? customerRepository = null,
        IVehicleRepository? vehicleRepository = null,
        IPaymentGateway? paymentGateway = null)
    {
        PaymentGateway.Setup(p => p.Charge(
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .Returns("PAYMENT-123");

        PricingService pricing = new(Promos, TimeProvider);

        return new BookingService(
            customerRepository ?? Customers,
            vehicleRepository ?? Vehicles,
            Bookings,
            Promos,
            pricing,
            paymentGateway ?? PaymentGateway.Object,
            Sms,
            Email,
            AuditLog,
            ReferenceGenerator,
            TimeProvider);
    }
}

public class StubCustomerRepository : ICustomerRepository
{
    public Dictionary<int, Customer> Data { get; } = new();

    public Customer? GetById(int id) =>
        Data.TryGetValue(id, out Customer? customer) ? customer : null;

    public List<Customer> GetAll() => Data.Values.ToList();

    public void Update(Customer customer) => Data[customer.Id] = customer;
}

public class StubVehicleRepository : IVehicleRepository
{
    public Dictionary<int, Vehicle> Data { get; } = new();

    public Vehicle? GetById(int id) =>
        Data.TryGetValue(id, out Vehicle? vehicle) ? vehicle : null;

    public List<Vehicle> GetAll() => Data.Values.ToList();

    public void Update(Vehicle vehicle) => Data[vehicle.Id] = vehicle;
}

public class StubBookingRepository : IBookingRepository
{
    public List<Booking> Data { get; } = new();

    public Booking? GetById(int id) => Data.FirstOrDefault(booking => booking.Id == id);

    public List<Booking> GetAll() => Data;

    public void Add(Booking booking) => Data.Add(booking);

    public void Update(Booking booking)
    {
    }

    public List<Booking> GetOpenBookingsForVehicle(int vehicleId) =>
        Data.Where(booking =>
            booking.VehicleId == vehicleId &&
            booking.Status is BookingStatus.Reserved or BookingStatus.Active).ToList();

    public int CountOpenBookingsForCustomer(int customerId) =>
        Data.Count(booking =>
            booking.CustomerId == customerId &&
            booking.Status is BookingStatus.Reserved or BookingStatus.Active);
}

public class StubPromoCodeRepository : IPromoCodeRepository
{
    public Dictionary<string, PromoCode> Data { get; } = new();

    public PromoCode? GetByCode(string code)
    {
        return Data.GetValueOrDefault(code);
    }

    public void Update(PromoCode promo)
    {
        Data[promo.Code] = promo;
    }
}


public class StubPaymentGateway : IPaymentGateway
{
    public string Charge(int customerId, decimal amount, string description)
    {
        return "PAYMENT-123";
    }

    public void Refund(int customerId, decimal amount, string description)
    {
    }
}

public class StubSmsSender : ISmsSender
{
    public void Send(string phone, string text)
    {
    }
}

public class StubEmailSender : IEmailSender
{
    public void Send(string to, string subject, string body)
    {
    }
}

public class StubAuditLog : IAuditLog
{
    public void Write(string message)
    {
    }
}

public class StubReferenceGenerator : IReferenceGenerator
{
    public string Generate()
    {
        return "AB12";
    }
}

public class SpySmsSender : ISmsSender
{
    public List<(string Phone, string Text)> SentMessages { get; } = new();

    public void Send(string phone, string text)
    {
        SentMessages.Add((phone, text));
    }
}

public class SpyEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> SentEmails { get; } = new();

    public void Send(string to, string subject, string body)
    {
        SentEmails.Add((to, subject, body));
    }
}

public class SpyAuditLog : IAuditLog
{
    public List<string> Messages { get; } = new();

    public void Write(string message)
    {
        Messages.Add(message);
    }
}

public class SpyPromoCodeRepository : IPromoCodeRepository
{
    public Dictionary<string, PromoCode> Data { get; } = new();

    public List<PromoCode> UpdatedPromos { get; } = new();

    public PromoCode? GetByCode(string code)
    {
        return Data.GetValueOrDefault(code);
    }

    public void Update(PromoCode promo)
    {
        Data[promo.Code] = promo;
        UpdatedPromos.Add(promo);
    }
}


public class DummyReferenceGenerator : IReferenceGenerator
{
    public string Generate()
    {
        throw new InvalidOperationException(
            "DummyReferenceGenerator should never be called.");
    }
}

public class HandWrittenMockPaymentGateway : IPaymentGateway
{
    public int ExpectedCustomerId { get; set; }
    public decimal ExpectedAmount { get; set; }
    public string ExpectedDescription { get; set; } = string.Empty;

    public int ChargeCallCount { get; private set; }
    public bool ExpectCharge { get; set; } = true;

    public string Charge(int customerId, decimal amount, string description)
    {
        ChargeCallCount++;

        if (!ExpectCharge)
        {
            throw new InvalidOperationException(
                "Charge was not expected.");
        }

        if (customerId != ExpectedCustomerId ||
            amount != ExpectedAmount ||
            description != ExpectedDescription)
        {
            throw new InvalidOperationException(
                "Charge arguments did not match the expectation.");
        }

        return "PAYMENT-123";
    }

    public void Refund(int customerId, decimal amount, string description)
    {
    }

    public void Verify()
    {
        int expectedCalls = ExpectCharge ? 1 : 0;

        if (ChargeCallCount != expectedCalls)
        {
            throw new InvalidOperationException(
                $"Expected {expectedCalls} Charge calls, but got {ChargeCallCount}.");
        }
    }
}


