using DriveNow.Application.Models;
using DriveNow.Application.Repositories;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.ValueObjects;
using DriveNow.UnitTests.Fakers;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;
using Microsoft.Extensions.Time.Testing;

namespace DriveNow.UnitTests;

public class PricingServiceTests
{
    
    [Theory]
    [Trait("Rule", "BR-30")]
    [InlineData(100.00, 1, 100.00)]
    [InlineData(200.00, 3, 600.00)]
    [InlineData(500.50, 2, 1001.00)]
    [InlineData(99.99, 3, 299.97)]
    public void Calculate_BaseAmount_ReturnsDailyRateMultipliedByRentalDays(double dailyRate, int rentalDays, double expectedBaseAmount)
    {
        // Arrange
        IPromoCodeRepository promoRepository = new FakePromoCodeRepository();
        PricingService sut = new PricingService(promoRepository, TimeProvider.System);

        Customer customer = new Customer
        {
            DateOfBirth = new DateTime(1990, 1, 1),
            LoyaltyTier = LoyaltyTier.None
        };

        Vehicle vehicle = new Vehicle
        {
            DailyRate = Convert.ToDecimal(dailyRate),
            Category = VehicleCategory.Economy
        };

        DateTime pickupAt = new DateTime(2026, 11, 1, 10, 0, 0);

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(rentalDays),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        // Act
        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        // Assert
        result.BaseAmount.Should().Be(Convert.ToDecimal(expectedBaseAmount));
    }

    private class FakePromoCodeRepository : IPromoCodeRepository
    {
        public PromoCode? GetByCode(string code)
        {
            return null;
        }

        public void Update(PromoCode promo)
        {
        }
    }
    
    [Theory]
    [Trait("Rule", "BR-31")]
    [InlineData(2026, 11, 1, 1, 0)]  // Sunday
    [InlineData(2026, 10, 30, 1, 20)] // Friday
    [InlineData(2026, 10, 31, 1, 20)] // Saturday
    [InlineData(2026, 10, 29, 3, 40)] // Thursday, Friday, Saturday
    public void Calculate_WeekendSurcharge_ReturnsExpectedAmount(
        int year,
        int month,
        int day,
        int rentalDays,
        decimal expected)
    {
        IPromoCodeRepository promoRepository = new Mock<IPromoCodeRepository>().Object;

        PricingService sut = new PricingService(
            promoRepository,
            TimeProvider.System);

        Customer customer = new Customer
        {
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        Vehicle vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = VehicleCategory.Economy
        };

        DateTime pickup = new DateTime(year, month, day, 10, 0, 0);

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(rentalDays),
            InsurancePlan = InsurancePlan.Basic
        };

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        result.WeekendSurcharge.Should().Be(expected);
    }
    
    [Theory]
    [Trait("Rule", "BR-32")]
    [InlineData(20, 0)]
    [InlineData(21, 15)]
    [InlineData(23, 15)]
    [InlineData(24, 15)]
    [InlineData(25, 0)]
    public void Calculate_YoungDriver_ReturnsExpectedSurcharge(
        int age,
        decimal expectedPercentage)
    {
        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        Customer customer = new Customer
        {
            DateOfBirth = pickup.AddYears(-age),
            LoyaltyTier = LoyaltyTier.None
        };

        Vehicle vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = VehicleCategory.Economy
        };

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(2),
            InsurancePlan = InsurancePlan.Basic
        };

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        decimal expected = result.BaseAmount * expectedPercentage / 100m;

        result.YoungDriverSurcharge.Should().Be(expected);
    }
    
    
    [Theory]
    [Trait("Rule", "BR-33")]
    [InlineData(3, 0)]
    [InlineData(6, 0)]
    [InlineData(7, 10)]
    [InlineData(13, 10)]
    [InlineData(14, 15)]
    public void Calculate_Duration_ReturnsExpectedDiscount(
        int rentalDays,
        decimal expectedPercentage)
    {
        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        Customer customer = new Customer
        {
            DateOfBirth = pickup.AddYears(-30),
            LoyaltyTier = LoyaltyTier.None
        };

        Vehicle vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = VehicleCategory.Economy
        };

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(rentalDays),
            InsurancePlan = InsurancePlan.Basic
        };

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        decimal expected = result.BaseAmount * expectedPercentage / 100m;

        result.DurationDiscount.Should().Be(expected);
    }
    
    
    [Theory]
    [Trait("Rule", "BR-34")]
    [InlineData(LoyaltyTier.None, 0)]
    [InlineData(LoyaltyTier.Silver, 5)]
    [InlineData(LoyaltyTier.Gold, 10)]
    [InlineData(LoyaltyTier.Platinum, 15)]
    public void Calculate_LoyaltyDiscount_ReturnsExpectedAmount(LoyaltyTier tier, decimal expectedPercentage)
    {
        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        Customer customer = new Customer
        {
            DateOfBirth = pickup.AddYears(-30),
            LoyaltyTier = tier
        };

        Vehicle vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = VehicleCategory.Economy
        };

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(2),
            InsurancePlan = InsurancePlan.Basic
        };

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        decimal expected = result.BaseAmount * expectedPercentage / 100m;

        result.LoyaltyDiscount.Should().Be(expected);
    }

    
    
    
    
    public class PromoCodeTestData : TheoryData<string, PromoCode?, int, VehicleCategory, decimal?, string?>
{
    public PromoCodeTestData()
    {
        Add(
            "SAVE10",
            new PromoCode
            {
                Code = "SAVE10",
                DiscountPercent = 10,
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 1,
                MaxUses = 100,
                UsedCount = 0
            },
            2,
            VehicleCategory.Economy,
            20m,
            null);

        Add(
            "UNKNOWN",
            null,
            3,
            VehicleCategory.Economy,
            null,
            "Promo code 'UNKNOWN' does not exist");

        Add(
            "EXPIRED",
            new PromoCode
            {
                Code = "EXPIRED",
                DiscountPercent = 10,
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = new DateTime(2026, 1, 31),
                MinDays = 1,
                MaxUses = 100
            },
            3,
            VehicleCategory.Economy,
            null,
            "Promo code 'EXPIRED' has expired");

        Add(
            "MIN DAYS",
            new PromoCode
            {
                Code = "MIN DAYS",
                DiscountPercent = 10,
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 5,
                MaxUses = 100
            },
            3,
            VehicleCategory.Economy,
            null,
            "Promo code 'MIN DAYS' requires at least 5 days");

        Add(
            "SUV ONLY",
            new PromoCode
            {
                Code = "SUV ONLY",
                DiscountPercent = 10,
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 1,
                AllowedCategories = new List<VehicleCategory>
                {
                    VehicleCategory.SUV
                },
                MaxUses = 100
            },
            3,
            VehicleCategory.Economy,
            null,
            "Promo code 'SUV ONLY' is not valid for Economy");

        Add(
            "USED UP",
            new PromoCode
            {
                Code = "USED UP",
                DiscountPercent = 10,
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 1,
                MaxUses = 5,
                UsedCount = 5
            },
            3,
            VehicleCategory.Economy,
            null,
            "Promo code 'USED UP' has been fully used");
        Add(
            "START DATE",
            new PromoCode
            {
                Code = "START DATE",
                DiscountPercent = 10,
                ValidFrom = new DateTime(2026, 11, 1),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 1,
                MaxUses = 100
            },
            2,
            VehicleCategory.Economy,
            20m,
            null);

            // Valid on the last day of the validity period
        Add(
            "END DATE",
            new PromoCode
            {
                Code = "END DATE",
                DiscountPercent = 10,
                ValidFrom = new DateTime(2026, 10, 1),
                ValidUntil = new DateTime(2026, 11, 1),
                MinDays = 1,
                MaxUses = 100
            },
            2,
            VehicleCategory.Economy,
            20m,
            null);

            // Null AllowedCategories means all vehicle categories are allowed
        Add(
            "ALL CATEGORIES",
            new PromoCode
            {
                Code = "ALL CATEGORIES",
                DiscountPercent = 10,
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = new DateTime(2026, 12, 31),
                MinDays = 1,
                AllowedCategories = null,
                MaxUses = 100
            },
            2,
            VehicleCategory.Luxury,
            20m,
            null);

            // Expired and below minimum days: expiration must be reported first
        Add(
            "MULTIPLE FAILURES",
            new PromoCode
            {
                Code = "MULTIPLE FAILURES",
                DiscountPercent = 10,
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = new DateTime(2026, 10, 31),
                MinDays = 5,
                MaxUses = 100
            },
            2,
            VehicleCategory.Economy,
            null,
            "Promo code 'MULTIPLE FAILURES' has expired");
    }
}
    
    
    [Theory]
    [Trait("Rule", "BR-35")]
    [Trait("Rule", "BR-36")]
    [ClassData(typeof(PromoCodeTestData))]
    public void Calculate_PromoCode_ReturnsExpectedResult(
        string promoCode,
        PromoCode? promo,
        int rentalDays,
        VehicleCategory category,
        decimal? expectedDiscount,
        string? expectedMessage)
    {
        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        promoRepository
            .Setup(repository => repository.GetByCode(promoCode))
            .Returns(promo);

        FakeTimeProvider fakeTimeProvider = new FakeTimeProvider(
            new DateTimeOffset(2026, 11, 1, 10, 0, 0, TimeSpan.Zero));

        PricingService sut = new PricingService(
            promoRepository.Object,
            fakeTimeProvider);

        Customer customer = new Customer
        {
            DateOfBirth = pickup.AddYears(-30),
            LoyaltyTier = LoyaltyTier.None
        };

        Vehicle vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = category
        };

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(rentalDays),
            InsurancePlan = InsurancePlan.Basic,
            PromoCode = promoCode
        };

        if (expectedMessage is not null)
        {
            Action act = () => sut.Calculate(customer, vehicle, request);

            act.Should()
                .Throw<InvalidPromoCodeException>()
                .WithMessage(expectedMessage);

            return;
        }

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        result.PromoDiscount.Should().Be(expectedDiscount);
    }
    
    
   
    [Theory]
    [Trait("Rule", "BR-37")]
    [InlineData(5, 40)]   // 20%: Below the cap
    [InlineData(10, 80)]  // 25%: Exactly at the cap
    [InlineData(15, 80)]  // 30%: Promo reduced to 10%
    public void Calculate_DiscountsRespect25PercentCap_ReducesPromoOnly(
        int promoPercent,
        decimal expectedPromoDiscount)
    {
        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        PromoCode promo = new PromoCode
        {
            Code = "SAVE15",
            DiscountPercent = promoPercent,
            ValidFrom = new DateTime(2026, 1, 1),
            ValidUntil = new DateTime(2026, 12, 31),
            MinDays = 1,
            MaxUses = 100
        };

        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        promoRepository
            .Setup(repository => repository.GetByCode("SAVE15"))
            .Returns(promo);

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        Customer customer = new Customer
        {
            DateOfBirth = pickup.AddYears(-30),
            LoyaltyTier = LoyaltyTier.Silver
        };

        Vehicle vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = VehicleCategory.Economy
        };

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(8),
            InsurancePlan = InsurancePlan.Basic,
            PromoCode = "SAVE15"
        };

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        result.DurationDiscount.Should().Be(80m);
        result.LoyaltyDiscount.Should().Be(40m);
        result.PromoDiscount.Should().Be(expectedPromoDiscount);
    }
    
    
    [Theory]
    [Trait("Rule", "BR-38")]
    [InlineData(InsurancePlan.Basic, VehicleCategory.Economy, 0)]
    [InlineData(InsurancePlan.Standard, VehicleCategory.Economy, 300)]
    [InlineData(InsurancePlan.Premium, VehicleCategory.Economy, 600)]
    [InlineData(InsurancePlan.Standard, VehicleCategory.Luxury, 600)]
    public void Calculate_Insurance_ReturnsExpectedCost(InsurancePlan plan, VehicleCategory category, decimal expected)
    {
        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        Customer customer = new Customer
        {
            DateOfBirth = pickup.AddYears(-30),
            LoyaltyTier = LoyaltyTier.None
        };

        Vehicle vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = category
        };

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(2),
            InsurancePlan = plan
        };

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        result.InsuranceCost.Should().Be(expected);
    }
    
    
    [Theory]
    [Trait("Rule", "BR-39")]
    [InlineData(LoyaltyTier.None, 1, 200)]
    [InlineData(LoyaltyTier.Gold, 1, 0)]
    [InlineData(LoyaltyTier.Gold, 2, 200)]
    [InlineData(LoyaltyTier.Platinum, 2, 200)]
    [InlineData(LoyaltyTier.Silver, 1, 200)]
    public void Calculate_AdditionalDrivers_ReturnsExpectedCost(LoyaltyTier tier,int additionalDrivers,decimal expected)
    {
        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        Customer customer = new Customer
        {
            DateOfBirth = pickup.AddYears(-30),
            LoyaltyTier = tier
        };

        Vehicle vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = VehicleCategory.Economy
        };

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(2),
            AdditionalDrivers = additionalDrivers
        };

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        result.AdditionalDriversCost.Should().Be(expected);
    }



    
   public static IEnumerable<object[]> PricingExamples
    {
    get
    {
        // Example A
        DateTime pickupA = new DateTime(2026, 11, 1, 10, 0, 0);

        yield return new object[]
        {
            new Customer
            {
                DateOfBirth = pickupA.AddYears(-34),
                LoyaltyTier = LoyaltyTier.Gold
            },
            new Vehicle
            {
                DailyRate = 600m,
                Category = VehicleCategory.Economy
            },
            new BookingRequest
            {
                PickupAt = pickupA,
                ReturnAt = pickupA.AddDays(3),
                InsurancePlan = InsurancePlan.Standard,
                AdditionalDrivers = 0
            },
            new PriceBreakdown
            {
                RentalDays = 3,
                BaseAmount = 1800m,
                WeekendSurcharge = 0m,
                YoungDriverSurcharge = 0m,
                DurationDiscount = 0m,
                LoyaltyDiscount = 180m,
                PromoDiscount = 0m,
                InsuranceCost = 450m,
                AdditionalDriversCost = 0m,
                Total = 2070m
            }
        };

        // Example B
        DateTime pickupB = new DateTime(2026, 11, 5, 9, 0, 0);

        yield return new object[]
        {
            new Customer
            {
                DateOfBirth = pickupB.AddYears(-23),
                LoyaltyTier = LoyaltyTier.Silver
            },
            new Vehicle
            {
                DailyRate = 1500m,
                Category = VehicleCategory.SUV
            },
            new BookingRequest
            {
                PickupAt = pickupB,
                ReturnAt = pickupB.AddDays(7).AddMinutes(30),
                InsurancePlan = InsurancePlan.Premium,
                AdditionalDrivers = 1,
                PromoCode = "SAVE15"
            },
            new PriceBreakdown
            {
                RentalDays = 7,
                BaseAmount = 10500m,
                WeekendSurcharge = 600m,
                YoungDriverSurcharge = 1575m,
                DurationDiscount = 1050m,
                LoyaltyDiscount = 525m,
                PromoDiscount = 1050m,
                InsuranceCost = 2100m,
                AdditionalDriversCost = 700m,
                Total = 12850m
            }
        };
    }
}
    
    
    [Theory]
    [Trait("Rule", "BR-40")]
    [MemberData(nameof(PricingExamples))]
    public void Calculate_FullPricingExample_ReturnsExpectedBreakdown(
        Customer customer,
        Vehicle vehicle,
        BookingRequest request,
        PriceBreakdown expected)
    {
        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        PromoCode promo = new PromoCode
        {
            Code = "SAVE15",
            DiscountPercent = 15,
            ValidFrom = new DateTime(2026, 1, 1),
            ValidUntil = new DateTime(2026, 12, 31),
            MinDays = 1,
            MaxUses = 100,
            UsedCount = 0
        };

        promoRepository
            .Setup(repository => repository.GetByCode("SAVE15"))
            .Returns(promo);

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        using (new AssertionScope())
        {
            result.Should().BeEquivalentTo(expected);
        }
    }
    
    [Fact]
    [Trait("Rule", "BR-40")]
    public void Calculate_WithRandomCustomerAndVehicle_ReturnsPriceBreakdown()
    {
        Customer customer = new CustomerFaker().Generate();
        Vehicle vehicle = new VehicleFaker().Generate();

        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(3),
            InsurancePlan = InsurancePlan.Basic
        };

        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        result.Should().NotBeNull();
    }
    
    [Fact]
    [Trait("Rule", "BR-32")]
    public void Calculate_WithCustomerAge20_DoesNotApplyYoungDriverSurcharge()
    {
        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        Customer customer = new CustomerFaker()
            .WithAge(20)
            .WithTier(LoyaltyTier.Silver)
            .Generate();

        customer.DateOfBirth = pickup.AddYears(-20);

        Vehicle vehicle = new VehicleFaker().Generate();
        vehicle.DailyRate = 100m;

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(2),
            InsurancePlan = InsurancePlan.Basic
        };

        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        result.YoungDriverSurcharge.Should().Be(0m);
    }
    
    



    [Fact]
    [Trait("Rule", "BR-30")]
    public void Calculate_WithFakerGeneratedData_ReturnsValidBreakdown()
    {
        Customer customer = new CustomerFaker()
            .WithAge(30)
            .WithTier(LoyaltyTier.None)
            .Generate();

        Vehicle vehicle = new VehicleFaker().Generate();

        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        BookingRequest request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(2),
            InsurancePlan = InsurancePlan.Basic
        };

        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        result.RentalDays.Should().Be(2);
        result.BaseAmount.Should().Be(Math.Round(vehicle.DailyRate * 2, 2, MidpointRounding.AwayFromZero));
    }

    
    
    
    [Theory]
    [MemberData(nameof(PricingExamples))]
    public async Task Calculate_FullPricingExample_MatchesSnapshot(
        Customer customer,
        Vehicle vehicle,
        BookingRequest request,
        PriceBreakdown expected)
    {
        Mock<IPromoCodeRepository> promoRepository =
            new Mock<IPromoCodeRepository>();

        PromoCode promo = new PromoCode
        {
            Code = "SAVE15",
            DiscountPercent = 15,
            ValidFrom = new DateTime(2026, 1, 1),
            ValidUntil = new DateTime(2026, 12, 31),
            MinDays = 1,
            MaxUses = 100,
            UsedCount = 0
        };

        promoRepository
            .Setup(repository => repository.GetByCode("SAVE15"))
            .Returns(promo);

        PricingService sut = new PricingService(
            promoRepository.Object,
            TimeProvider.System);

        PriceBreakdown result = sut.Calculate(customer, vehicle, request);

        await Verify(result).UseParameters(request.PromoCode ?? "ExampleA");
        using (new AssertionScope())
        {
            result.Should().BeEquivalentTo(expected);
        }

    }

    
}