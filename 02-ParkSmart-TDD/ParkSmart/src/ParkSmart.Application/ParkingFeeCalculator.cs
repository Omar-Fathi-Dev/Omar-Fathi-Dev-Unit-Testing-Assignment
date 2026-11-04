using ParkSmart.Domain;

namespace ParkSmart.Application;

public class ParkingFeeCalculator
{
    const int FreeParkingDurationInMinutes = 15;

    public decimal Calculate(ParkingSession session)
    {
        TimeSpan parkingDuration = session.ExitTime - session.EntryTime;

        if (parkingDuration.TotalMinutes <= FreeParkingDurationInMinutes)
        {
            return 0m;
        }

        return 10m;
    }
}