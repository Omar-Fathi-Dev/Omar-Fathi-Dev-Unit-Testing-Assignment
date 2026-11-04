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