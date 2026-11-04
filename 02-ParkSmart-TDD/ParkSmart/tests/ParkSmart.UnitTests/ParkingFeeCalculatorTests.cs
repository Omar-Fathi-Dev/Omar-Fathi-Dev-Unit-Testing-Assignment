using ParkSmart.Application;
using ParkSmart.Domain;

namespace ParkSmart.UnitTests;

public class ParkingFeeCalculatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    public void Calculate_ParkingDurationOfFifteenMinutesOrLess_ReturnsZero(int durationInMinutes)
    {
        ParkingFeeCalculator calculator = new();

        ParkingSession session = CreateSession(durationInMinutes);

        decimal result = calculator.Calculate(session);

        result.Should().Be(0m);
    }

    [Fact]
    public void Calculate_ParkingDurationOfFifteenMinutesAndOneSecond_ReturnsPositiveFee()
    {
        ParkingFeeCalculator calculator = new();

        ParkingSession session = CreateSession(15);
        session.ExitTime = session.EntryTime.AddMinutes(15).AddSeconds(1);

        decimal result = calculator.Calculate(session);

        result.Should().BeGreaterThan(0m);
    }
    
    
    [Theory]
    [InlineData(16, 10)]
    [InlineData(60, 10)]
    [InlineData(61, 20)]
    [InlineData(180, 30)]
    public void Calculate_ParkingDuration_ReturnsFeePerStartedHour(int durationInMinutes, decimal expectedFee)
    {
        ParkingFeeCalculator calculator = new();

        ParkingSession session = CreateSession(durationInMinutes);

        decimal result = calculator.Calculate(session);

        result.Should().Be(expectedFee);
    }
    
    [Fact]
    public void Calculate_ExitTimeEqualsEntryTime_ReturnsZero()
    {
        ParkingFeeCalculator calculator = new();

        ParkingSession session = CreateSession(0);

        decimal result = calculator.Calculate(session);

        result.Should().Be(0m);
    }
    
    
    [Fact]
    public void Calculate_ExitTimeBeforeEntryTime_ThrowsArgumentException()
    {
        ParkingFeeCalculator calculator = new();

        ParkingSession session = CreateSession(0);
        session.ExitTime = session.EntryTime.AddMinutes(-1);

        Action act = () => calculator.Calculate(session);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Exit time must be after entry time.");
    }
    
    
    [Theory]
    [InlineData(600, 80)]
    [InlineData(1800, 140)]
    [InlineData(2880, 160)]
    [InlineData(2890, 170)]
    public void Calculate_ParkingAcrossDailyBlocks_AppliesDailyCap(int durationInMinutes, decimal expectedFee)
    {
        ParkingFeeCalculator calculator = new();

        ParkingSession session = CreateSession(durationInMinutes);

        decimal result = calculator.Calculate(session);

        result.Should().Be(expectedFee);
    }
    
    [Theory]
    [InlineData(VehicleType.Motorcycle, 180, 15)]
    [InlineData(VehicleType.Truck, 600, 200)]
    [InlineData(VehicleType.Truck, 1800, 350)]
    public void Calculate_VehicleType_ReturnsCorrectFee(VehicleType vehicle, int durationInMinutes, decimal expectedFee)
    {
        ParkingFeeCalculator calculator = new();

        ParkingSession session = CreateSession(durationInMinutes);
        session.Vehicle = vehicle;

        decimal result = calculator.Calculate(session);

        result.Should().Be(expectedFee);
    }
    
    
    [Theory]
    [InlineData(23, 0, 1, 7, 0, VehicleType.Car, 30)]
    [InlineData(23, 0, 1, 7, 0, VehicleType.Motorcycle, 15)]
    [InlineData(23, 0, 1, 7, 0, VehicleType.Truck, 75)]
    [InlineData(22, 0, 0, 23, 0, VehicleType.Car, 10)]
    [InlineData(21, 59, 1, 7, 0, VehicleType.Car, 80)]
    [InlineData(23, 0, 1, 8, 1, VehicleType.Car, 80)]
    public void Calculate_NightParking_ReturnsLowerOfNormalAndFlatRate(
        int entryHour,
        int entryMinute,
        int exitDayOffset,
        int exitHour,
        int exitMinute,
        VehicleType vehicle,
        decimal expectedFee)
    {
        ParkingFeeCalculator calculator = new();

        DateTime entryTime = new(2026, 1, 5, entryHour, entryMinute, 0);

        ParkingSession session = new()
        {
            EntryTime = entryTime,
            ExitTime = entryTime.Date
                .AddDays(exitDayOffset)
                .AddHours(exitHour)
                .AddMinutes(exitMinute),
            Vehicle = vehicle
        };

        decimal result = calculator.Calculate(session);

        result.Should().Be(expectedFee);
    }
    
    
    
    [Theory]
    [InlineData(VehicleType.Car, 5, 150)]
    [InlineData(VehicleType.Car, 1800, 150)]
    [InlineData(VehicleType.Truck, 1800, 350)]
    [InlineData(VehicleType.Motorcycle, 5, 75)]
    public void Calculate_LostTicket_AppliesMinimumFee(VehicleType vehicle, int durationInMinutes, decimal expectedFee)
    {
        ParkingFeeCalculator calculator = new();

        ParkingSession session = CreateSession(durationInMinutes);
        session.Vehicle = vehicle;
        session.TicketLost = true;

        decimal result = calculator.Calculate(session);

        result.Should().Be(expectedFee);
    }
    
    
    
    
    
    
    
    
    
    
    
    
    private static ParkingSession CreateSession(int durationInMinutes)
    {
        DateTime entryTime = new(2026, 1, 5, 10, 0, 0);

        return new ParkingSession
        {
            EntryTime = entryTime,
            ExitTime = entryTime.AddMinutes(durationInMinutes)
        };
    }
}