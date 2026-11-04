using ParkSmart.Domain;

namespace ParkSmart.Application;

public class ParkingFeeCalculator
{
    public decimal Calculate(ParkingSession session)
    {
        if (session.ExitTime < session.EntryTime)
        {
            throw new ArgumentException(
                "Exit time must be after entry time.");
        }
        
        TimeSpan duration = session.ExitTime - session.EntryTime;

        if (duration <= TimeSpan.FromMinutes(15))
        {
            return 0m;
        }
        double startedHours = Math.Ceiling(duration.TotalHours);
        return (decimal)startedHours * 10m;
    }
}