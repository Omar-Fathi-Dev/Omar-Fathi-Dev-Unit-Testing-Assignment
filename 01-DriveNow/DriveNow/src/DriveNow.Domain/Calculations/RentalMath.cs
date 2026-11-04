namespace DriveNow.Domain.Calculations;

public static class RentalMath
{
    public static int CalculateAge(DateTime dateOfBirth, DateTime onDate) =>
        FullYears(dateOfBirth, onDate);

    public static int CalculateLicenseYears(DateTime issueDate, DateTime onDate) =>
        FullYears(issueDate, onDate);

    public static int CalculateRentalDays(DateTime pickup, DateTime returnAt)
    {
        if (returnAt <= pickup)
            throw new ArgumentException("Return must be after pickup");

        TimeSpan span = returnAt - pickup;
        int days = (int)(span.Ticks / TimeSpan.TicksPerDay);
        TimeSpan remainder = TimeSpan.FromTicks(span.Ticks % TimeSpan.TicksPerDay);

        if (remainder.TotalMinutes > 60)
            days++;

        return Math.Max(days, 1);
    }

    private static int FullYears(DateTime start, DateTime onDate)
    {
        int years = onDate.Year - start.Year;

        if (onDate.Date < Birthday(start, onDate.Year))
            years--;

        return years;
    }

    private static DateTime Birthday(DateTime start, int year)
    {
        int day = start.Day;

        if (start.Month == 2 && day == 29 && !DateTime.IsLeapYear(year))
            day = 28;

        return new DateTime(year, start.Month, day);
    }
}