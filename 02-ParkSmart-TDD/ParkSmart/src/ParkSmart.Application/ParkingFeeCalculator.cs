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
        decimal hourlyRate;
        decimal dailyCap;
        decimal nightFlatRate;
        decimal lostTicketMinimum;

        switch (session.Vehicle)
        {
            case VehicleType.Motorcycle:
                hourlyRate = 5m;
                dailyCap = 40m;
                nightFlatRate = 15m;
                lostTicketMinimum = 75m;
                break;

            case VehicleType.Truck:
                hourlyRate = 25m;
                dailyCap = 200m;
                nightFlatRate = 75m;
                lostTicketMinimum = 300m;
                break;

            default:
                hourlyRate = 10m;
                dailyCap = 80m;
                nightFlatRate = 30m;
                lostTicketMinimum = 150m;
                break;
        }

        TimeSpan freePeriod = session.EntryTime.DayOfWeek == DayOfWeek.Friday
            ? TimeSpan.FromHours(2)
            : TimeSpan.FromMinutes(15);
        decimal fee = 0m;

        if (duration > freePeriod || session.TicketLost)
        {
            int startedHours = (int)Math.Ceiling(duration.TotalHours);
            while (startedHours > 0)
            {
                int blockHours = Math.Min(startedHours, 24);
                decimal blockFee = Math.Min(blockHours * hourlyRate, dailyCap);
                fee += blockFee;
                startedHours -= blockHours;
            }

            DateTime nightEndTime = session.EntryTime.Date.AddDays(1).AddHours(8);
            bool isNightParking = session.EntryTime.TimeOfDay >= TimeSpan.FromHours(22) &&
                                  session.ExitTime <= nightEndTime;
            if (isNightParking)
            {
                fee = Math.Min(fee, nightFlatRate);
            }
        }

        if (session.IsSubscriber)
        {
            bool isSubscriberFree =
                session.EntryTime.Date == session.ExitTime.Date &&
                session.EntryTime.DayOfWeek >= DayOfWeek.Sunday &&
                session.EntryTime.DayOfWeek <= DayOfWeek.Thursday &&
                session.EntryTime.TimeOfDay >= TimeSpan.FromHours(7) &&
                session.ExitTime.TimeOfDay <= TimeSpan.FromHours(19);

            fee = isSubscriberFree ? 0m : fee * 0.5m;
        }

        if (session.TicketLost)
        {
            fee = Math.Max(fee, lostTicketMinimum);
        }

        return fee;
    }
}