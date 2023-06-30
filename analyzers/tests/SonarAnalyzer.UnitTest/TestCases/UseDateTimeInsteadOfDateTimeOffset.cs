using System;
using System.Globalization;

public class Program
{
    void Constructors()
    {
        _ = new DateTime(1);                                                                 // Noncompliant
        _ = new DateTime(1, 1, 1);                                                           // Noncompliant
        _ = new DateTime(1, 1, 1, new GregorianCalendar());                                  // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1);                                                  // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, new GregorianCalendar());                         // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, DateTimeKind.Utc);                                // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1);                                               // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1, new GregorianCalendar());                      // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);                             // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1, new GregorianCalendar(), DateTimeKind.Utc);    // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1, 1, new GregorianCalendar());                   // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);                          // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1, 1, new GregorianCalendar(), DateTimeKind.Utc); // Noncompliant
    }

    void Fields()
    {
        _ = DateTime.MaxValue;  // Noncompliant
        _ = DateTime.MinValue;  // Noncompliant
        _ = DateTime.UnixEpoch; // Noncompliant
    }

    void Properties(DateTime date)
    {
        _ = date.Date;
        _ = date.Day;
        _ = date.DayOfWeek;
        _ = date.DayOfYear;
        _ = date.Hour;
        _ = date.Kind;
        _ = date.Microsecond;
        _ = date.Millisecond;
        _ = date.Minute;
        _ = date.Month;
        _ = date.Nanosecond;
        _ = date.Second;
        _ = date.Ticks;
        _ = date.TimeOfDay;
        _ = date.Year;
    }

    void StaticProperties()
    {
        _ = DateTime.Now; // Noncompliant
        _ = DateTime.Today; // Noncompliant
        _ = DateTime.UtcNow; // Noncompliant
    }

    void Methods(DateTime date, Span<char> span)
    {
        date.Add(TimeSpan.Zero);
        date.AddDays(0);
        date.AddHours(0);
        date.AddMicroseconds(0);
        date.AddMilliseconds(0);
        date.AddMinutes(0);
        date.AddMonths(0);
        date.AddSeconds(0);
        date.AddTicks(0);
        date.AddYears(0);
        date.CompareTo(date);
        DateTime.Compare(date, date);
        DateTime.DaysInMonth(1, 1);
        DateTime.Equals(date, date);
        date.Equals(date);
        DateTime.FromBinary(1);
        DateTime.FromFileTime(1);
        DateTime.FromFileTimeUtc(1);
        DateTime.FromOADate(1);
        date.GetDateTimeFormats('a');
        date.GetHashCode();
        date.GetTypeCode();
        date.IsDaylightSavingTime();
        DateTime.IsLeapYear(1);
        DateTime.Parse("06/01/1993");                          // Noncompliant
        DateTime.ParseExact("06/01/1993", "dd/MM/yyyy", null); // Noncompliant
        DateTime.SpecifyKind(date, DateTimeKind.Local);        // Noncompliant
        date.Subtract(date);
        date.ToBinary();
        date.ToFileTime();
        date.ToFileTimeUtc();
        date.ToLocalTime();
        date.ToLongDateString();
        date.ToLongTimeString();
        date.ToShortDateString();
        date.ToShortTimeString();
        date.ToString();
        date.ToUniversalTime();
        date.TryFormat(span, out int myInt);
        DateTime.TryParse("06/01/1993", out date);                                                                                    // Noncompliant
        DateTime.TryParseExact("06/01/1993", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out date); // Noncompliant
    }
}
