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
    
    
    [Theory]
    [InlineData(600, 80)]
    [InlineData(1800, 140)]
    [InlineData(2880, 160)]
    [InlineData(2890, 170)]
    public void Calculate_ParkingAcrossDailyBlocks_AppliesDailyCap(int minutes, decimal expectedFee)
    {
        ParkingSession session = CreateSession(minutes);
        ParkingFeeCalculator calculator = new ParkingFeeCalculator();

        decimal result = calculator.Calculate(session);

        result.Should().Be(expectedFee);
    }
    
    
    [Theory]
    [InlineData(VehicleType.Motorcycle, 180, 15)]
    [InlineData(VehicleType.Truck, 600, 200)]
    [InlineData(VehicleType.Truck, 1800, 350)]
    public void Calculate_VehicleType_ReturnsCorrectFee(VehicleType vehicle, int minutes, decimal expectedFee)
    {
        ParkingSession session = CreateSession(minutes);
        session.Vehicle = vehicle;
        ParkingFeeCalculator calculator = new ParkingFeeCalculator();

        decimal result = calculator.Calculate(session);

        result.Should().Be(expectedFee);
    }
    
    
    [Theory]
    [InlineData(23, 0, 7, 0, 30)]
    [InlineData(22, 0, 23, 0, 10)]
    [InlineData(21, 59, 7, 0, 80)]
    [InlineData(23, 0, 8, 1, 80)]
    public void Calculate_NightParking_ReturnsLowerOfNormalAndFlatRate(int entryHour, int entryMinute, int exitHour, int exitMinute, decimal expectedFee)
    {
        DateTime entryTime = new DateTime(2026, 11, 1, entryHour, entryMinute, 0);

        DateTime exitTime = new DateTime(2026, 11, 1, exitHour, exitMinute, 0);

        if (exitTime <= entryTime)
        {
            exitTime = exitTime.AddDays(1);
        }
        ParkingSession session = new ParkingSession
        {
            EntryTime = entryTime,
            ExitTime = exitTime
        };
        ParkingFeeCalculator calculator = new ParkingFeeCalculator();

        decimal result = calculator.Calculate(session);

        result.Should().Be(expectedFee);
    }
    
    
    [Theory]
    [InlineData(VehicleType.Car, 5, 150)]
    [InlineData(VehicleType.Car, 1800, 150)]
    [InlineData(VehicleType.Motorcycle, 5, 75)]
    [InlineData(VehicleType.Truck, 1800, 350)]
    public void Calculate_LostTicket_AppliesMinimumFee(VehicleType vehicle, int minutes, decimal expectedFee)
    {
        ParkingSession session = CreateSession(minutes);
        session.Vehicle = vehicle;
        session.TicketLost = true;
        ParkingFeeCalculator calculator = new ParkingFeeCalculator();
        
        decimal result = calculator.Calculate(session);

        result.Should().Be(expectedFee);
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