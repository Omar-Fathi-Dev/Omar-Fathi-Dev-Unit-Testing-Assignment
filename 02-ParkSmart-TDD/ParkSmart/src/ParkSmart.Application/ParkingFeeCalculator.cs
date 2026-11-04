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
        decimal nightFlatRate;

        switch (session.Vehicle)
        {
            case VehicleType.Motorcycle:
                hourlyRate = 5m;
                dailyCap = 40m;
                nightFlatRate = 15m;
                break;

            case VehicleType.Truck:
                hourlyRate = 25m;
                dailyCap = 200m;
                nightFlatRate = 75m;
                break;

            default:
                hourlyRate = 10m;
                dailyCap = 80m;
                nightFlatRate = 30m;
                break;
        }

        int startedHours = (int)Math.Ceiling(duration.TotalHours);
        decimal normalFee = 0m;
        while (startedHours > 0)
        {
            int blockHours = Math.Min(startedHours, 24);
            decimal blockFee = Math.Min(blockHours * hourlyRate, dailyCap);
            normalFee += blockFee;
            startedHours -= blockHours;
        }

        DateTime nightEndTime = session.EntryTime.Date.AddDays(1).AddHours(8);

        bool isNightParking = session.EntryTime.TimeOfDay >= TimeSpan.FromHours(22) && session.ExitTime <= nightEndTime;

        if (isNightParking)
        {
            return Math.Min(normalFee, nightFlatRate);
        }

        return normalFee;
    }
}