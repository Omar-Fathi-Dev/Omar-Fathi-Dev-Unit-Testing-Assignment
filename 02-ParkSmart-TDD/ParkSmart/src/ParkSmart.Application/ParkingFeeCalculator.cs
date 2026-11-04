using ParkSmart.Domain;

namespace ParkSmart.Application;

public class ParkingFeeCalculator
{
    private const int FreeParkingDurationInMinutes = 15;
    private const int DailyBlockDurationInMinutes = 1440;

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

        int remainingMinutes = (int)parkingDuration.TotalMinutes;
        decimal totalFee = 0m;

        while (remainingMinutes > 0)
        {
            int currentBlockMinutes = Math.Min(
                remainingMinutes,
                DailyBlockDurationInMinutes);

            decimal startedHours = (decimal)Math.Ceiling(
                currentBlockMinutes / 60m);

            decimal blockFee = startedHours * hourlyRate;

            totalFee += Math.Min(blockFee, dailyCap);

            remainingMinutes -= currentBlockMinutes;
        }

        return totalFee;
    }
}