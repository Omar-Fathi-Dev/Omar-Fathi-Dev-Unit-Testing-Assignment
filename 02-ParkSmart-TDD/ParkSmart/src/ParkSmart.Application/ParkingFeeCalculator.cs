using ParkSmart.Domain;

namespace ParkSmart.Application;

public class ParkingFeeCalculator
{
    private const int FreeParkingDurationInMinutes = 15;

    public decimal Calculate(ParkingSession session)
    {
        TimeSpan parkingDuration = session.ExitTime - session.EntryTime;

        if (parkingDuration.TotalMinutes <= FreeParkingDurationInMinutes)
        {
            return 0m;
        }

        decimal startedHours = (decimal)Math.Ceiling(
            parkingDuration.TotalMinutes / 60);

        return startedHours * 10m;
    }
}