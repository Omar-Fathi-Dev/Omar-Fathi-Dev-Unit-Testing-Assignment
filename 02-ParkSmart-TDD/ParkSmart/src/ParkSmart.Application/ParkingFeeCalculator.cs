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
        int startedHours = (int)Math.Ceiling(duration.TotalHours);
        decimal totalFee = 0m;
        while (startedHours > 0)
        {
            int blockHours = Math.Min(startedHours, 24);
            decimal blockFee = Math.Min(blockHours * 10m, 80m);
            totalFee += blockFee;
            startedHours -= blockHours;
        }

        return totalFee;
    }
}