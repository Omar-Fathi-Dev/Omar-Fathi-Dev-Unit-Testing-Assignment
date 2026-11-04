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

        (decimal hourlyRate, decimal dailyCap) =
            GetVehiclePricing(session.Vehicle);

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
            decimal nightFlatRate = GetNightFlatRate(session.Vehicle);

            return Math.Min(totalFee, nightFlatRate);
        }

        return totalFee;
    }

    private static (decimal HourlyRate, decimal DailyCap) GetVehiclePricing(
        VehicleType vehicle)
    {
        return vehicle switch
        {
            VehicleType.Motorcycle => (5m, 40m),
            VehicleType.Truck => (25m, 200m),
            _ => (10m, 80m)
        };
    }

    private static decimal GetNightFlatRate(VehicleType vehicle)
    {
        return vehicle switch
        {
            VehicleType.Motorcycle => 15m,
            VehicleType.Truck => 75m,
            _ => 30m
        };
    }

    private static bool IsNightParking(ParkingSession session)
    {
        return session.EntryTime.TimeOfDay >= TimeSpan.FromHours(22) &&
               session.ExitTime.Date == session.EntryTime.Date.AddDays(1) &&
               session.ExitTime.TimeOfDay <= TimeSpan.FromHours(8);
    }
}