using ParkSmart.Application;
using ParkSmart.Domain;

namespace ParkSmart.UnitTests;

public class ParkingFeeCalculatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    public void Calculate_ZeroAndFifteenMinutes_ReturnsZero(int minutes)
    {
        ParkingSession session = CreateSession(minutes);
        ParkingFeeCalculator calculator = new ParkingFeeCalculator();
        decimal result = calculator.Calculate(session);
        result.Should().Be(0);
    }

    [Fact]
    public void Calculate_SixteenMinutes_ReturnsPositiveFee()
    {
        ParkingSession session = CreateSession(16);
        ParkingFeeCalculator calculator = new ParkingFeeCalculator();
        decimal result = calculator.Calculate(session);
        result.Should().BeGreaterThan(0);
    }
    
    
    [Theory]
    [InlineData(16, 10)]
    [InlineData(60, 10)]
    [InlineData(61, 20)]
    [InlineData(180, 30)]
    public void Calculate_ParkingDuration_ReturnsFeePerStartedHour(int minutes, decimal expectedFee)
    {
        ParkingSession session = CreateSession(minutes);
        ParkingFeeCalculator calculator = new ParkingFeeCalculator();
        decimal result = calculator.Calculate(session);
        result.Should().Be(expectedFee);
    }
    
    
    [Fact]
    public void Calculate_ExitTimeEqualsEntryTime_ReturnsZero()
    {
        ParkingSession session = CreateSession(0);
        ParkingFeeCalculator calculator = new ParkingFeeCalculator();
        
        decimal result = calculator.Calculate(session);

        result.Should().Be(0);
    }

    [Fact]
    public void Calculate_ExitTimeBeforeEntryTime_ThrowsArgumentException()
    {
        DateTime entryTime = new DateTime(2026, 11, 1, 10, 0, 0);
        ParkingSession session = new ParkingSession
        {
            EntryTime = entryTime,
            ExitTime = entryTime.AddMinutes(-1)
        };

        ParkingFeeCalculator calculator = new ParkingFeeCalculator();

        Action act = () => calculator.Calculate(session);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Exit time must be after entry time.");
    }
    
    
    
    
    
    
    
    
    

    private static ParkingSession CreateSession(int durationInMinutes)
    {
        DateTime entryTime = new DateTime(2026, 11, 1, 10, 0, 0);
        return new ParkingSession
        {
            EntryTime = entryTime,
            ExitTime = entryTime.AddMinutes(durationInMinutes)
        };
    }
}