using ParkSmart.Domain;

namespace ParkSmart.Application;

public class ParkingFeeCalculator
{
    public decimal Calculate(ParkingSession session)
    {
        TimeSpan duration = session.ExitTime - session.EntryTime;

        if (duration <= TimeSpan.FromMinutes(15))
        {
            return 0m;
        }

        return 1m;
    }
}