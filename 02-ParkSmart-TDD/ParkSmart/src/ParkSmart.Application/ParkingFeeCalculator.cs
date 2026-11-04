using ParkSmart.Domain;

namespace ParkSmart.Application;

public class ParkingFeeCalculator
{
    public decimal Calculate(ParkingSession session)
    {
        TimeSpan parkingDuration = session.ExitTime - session.EntryTime;

        if (parkingDuration.TotalMinutes <= 15)
        {
            return 0m;
        }

        return 10m;
    }
}