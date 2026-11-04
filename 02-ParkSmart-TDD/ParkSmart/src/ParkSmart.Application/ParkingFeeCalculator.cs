using ParkSmart.Domain;

namespace ParkSmart.Application;

public class ParkingFeeCalculator
{
    public decimal Calculate(ParkingSession session)
    {
        if (session.ExitTime < session.EntryTime)
        {
            throw new ArgumentException("Exit time must be after entry time.");
        }
        
        TimeSpan duration = session.ExitTime - session.EntryTime;

        if (duration <= TimeSpan.FromMinutes(15))
        {
            return 0m;
        }
        decimal hourlyRate;
        decimal dailyCap;

        switch (session.Vehicle)
        {
            case VehicleType.Motorcycle:
                hourlyRate = 5m;
                dailyCap = 40m;
                break;

            case VehicleType.Truck:
                hourlyRate = 25m;
                dailyCap = 200m;
                break;

            default:
                hourlyRate = 10m;
                dailyCap = 80m;
                break;
        }

        int startedHours = (int)Math.Ceiling(duration.TotalHours);
        decimal totalFee = 0m;
        while (startedHours > 0)
        {
            int blockHours = Math.Min(startedHours, 24);
            decimal blockFee = Math.Min(blockHours * hourlyRate, dailyCap);
            totalFee += blockFee;
            startedHours -= blockHours;
        }

        return totalFee;
    }
}