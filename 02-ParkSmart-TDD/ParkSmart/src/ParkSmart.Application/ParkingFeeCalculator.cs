using ParkSmart.Domain;

namespace ParkSmart.Application;

public class ParkingFeeCalculator
{
    private const int FreeParkingDurationInMinutes = 15;
    private const int FridayFreeParkingDurationInMinutes = 120;
    private const int DailyBlockDurationInMinutes = 1440;

    public decimal Calculate(ParkingSession session)
    {
        if (session.ExitTime < session.EntryTime)
        {
            throw new ArgumentException(
                "Exit time must be after entry time.");
        }

        TimeSpan parkingDuration = session.ExitTime - session.EntryTime;

        int freeParkingDuration = GetFreeParkingDuration(session.EntryTime);

        if (!session.TicketLost &&
            parkingDuration.TotalMinutes <= freeParkingDuration)
        {
            return 0m;
        }

        (decimal hourlyRate,
         decimal dailyCap,
         decimal nightFlatRate,
         decimal lostTicketMinimum) = GetVehiclePricing(session.Vehicle);

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

        if (IsNightParking(session))
        {
            totalFee = Math.Min(totalFee, nightFlatRate);
        }

        if (session.TicketLost)
        {
            totalFee = Math.Max(totalFee, lostTicketMinimum);
        }

        return totalFee;
    }

    private static int GetFreeParkingDuration(DateTime entryTime)
    {
        return entryTime.DayOfWeek == DayOfWeek.Friday
            ? FridayFreeParkingDurationInMinutes
            : FreeParkingDurationInMinutes;
    }

    private static (
        decimal HourlyRate,
        decimal DailyCap,
        decimal NightFlatRate,
        decimal LostTicketMinimum) GetVehiclePricing(VehicleType vehicle)
    {
        return vehicle switch
        {
            VehicleType.Motorcycle => (5m, 40m, 15m, 75m),
            VehicleType.Truck => (25m, 200m, 75m, 300m),
            _ => (10m, 80m, 30m, 150m)
        };
    }

    private static bool IsNightParking(ParkingSession session)
    {
        return session.EntryTime.TimeOfDay >= TimeSpan.FromHours(22) &&
               session.ExitTime.Date == session.EntryTime.Date.AddDays(1) &&
               session.ExitTime.TimeOfDay <= TimeSpan.FromHours(8);
    }
}