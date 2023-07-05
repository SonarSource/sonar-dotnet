using System;
using System.Globalization;
using MyAlias = System.DateTime;

public class Program
{
    void Constructors()
    {
        _ = new DateTime(1);                                                                 // Noncompliant {{Prefer using "DateTimeOffset" instead of "DateTime"}}
//          ^^^^^^^^^^^^^^^
        _ = new DateTime(1, 1, 1);                                                           // Noncompliant
        _ = new DateTime(1, 1, 1, new GregorianCalendar());                                  // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1);                                                  // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, new GregorianCalendar());                         // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, DateTimeKind.Utc);                                // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1);                                               // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1, new GregorianCalendar());                      // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);                             // Noncompliant
        _ = new DateTime(1, 1, 1, 1, 1, 1, 1, new GregorianCalendar(), DateTimeKind.Utc);    // Noncompliant
        _ = new MyAlias(1);                                                                  // FN
        _ = new System.DateTime(1);                                                          // Noncompliant
    }

    void Fields()
    {
        _ = DateTime.MaxValue;  // Noncompliant
//          ^^^^^^^^
        _ = DateTime.MinValue;  // Noncompliant
    }

    void StaticProperties()
    {
        _ = DateTime.Now; // Noncompliant
//          ^^^^^^^^
        _ = DateTime.Today; // Noncompliant
        _ = DateTime.UtcNow; // Noncompliant
    }

    void Methods(DateTime date)
    {
        date.Add(TimeSpan.Zero);
        date.AddDays(0);
        date.AddHours(0);
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
        DateTime.Parse("06/01/1993");
        DateTime.ParseExact("06/01/1993", "dd/MM/yyyy", null);
        DateTime.SpecifyKind(date, DateTimeKind.Local);
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
        DateTime.TryParse("06/01/1993", out date);
        DateTime.TryParseExact("06/01/1993", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out date);
    }

    void EdgeCases()
    {
        var a = DateTime.Now.AddDays(-1).Ticks; // Noncompliant
        var b = new DateTimeOffset(DateTime.Now); // Noncompliant
        var typeName = nameof(DateTime); // Compliant
    }
}

public class FakeDateTime
{
    void MyMethod() => new DateTime(1); // Compliant

    public class DateTime
    {
        public DateTime(int ticks) { }
    }
}
