using ParkSmart.Domain;

namespace ParkSmart.Application;

public class ParkingFeeCalculator
{
    private const int FreeParkingDurationInMinutes = 15;
    private const decimal HourlyRate = 10m;
    private const int DailyBlockDurationInMinutes = 1440;
    private const decimal DailyCap = 80m;

    public decimal Calculate(ParkingSession session)
    {
        if (session.ExitTime < session.EntryTime)
        {
            throw new ArgumentException(
                "Exit time must be after entry time.");
        }

        TimeSpan parkingDuration = session.ExitTime - session.EntryTime;

        if (parkingDuration.TotalMinutes <= FreeParkingDurationInMinutes)
        {
            return 0m;
        }

        int remainingMinutes = (int)parkingDuration.TotalMinutes;
        decimal totalFee = 0m;

        while (remainingMinutes > 0)
        {
            int currentBlockMinutes = Math.Min(
                remainingMinutes,
                DailyBlockDurationInMinutes);

            totalFee += CalculateBlockFee(currentBlockMinutes);

            remainingMinutes -= currentBlockMinutes;
        }

        return totalFee;
    }

    private static decimal CalculateBlockFee(int durationInMinutes)
    {
        decimal startedHours = (decimal)Math.Ceiling(
            durationInMinutes / 60m);

        decimal blockFee = startedHours * HourlyRate;

        return Math.Min(blockFee, DailyCap);
    }
}