using System;
using System.Globalization;

public class DateTimeFormatShouldNotBeHardcoded
{
    private string Format = $"dd/MM";

    public void DateTimeCases()
    {
        var stringRepresentation = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"); // Noncompliant {{Do not hardcode the format specifier.}}
//                                                          ^^^^^^^^^^^^^^^^^^^^^
        stringRepresentation = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("es-MX")); // Noncompliant

        stringRepresentation = DateTime.UtcNow.ToString(Format); // FN
        stringRepresentation = DateTime.UtcNow.ToString(CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
        stringRepresentation = DateTime.UtcNow.ToString("d");
        stringRepresentation = DateTime.UtcNow.ToString("d", CultureInfo.GetCultureInfo("es-MX"));
    }

    public void DateTimeOffsetCases(DateTimeOffset dateTimeOffset)
    {
        var stringRepresentation = dateTimeOffset.ToString("dd/MM/yyyy HH:mm:ss"); // Noncompliant
        stringRepresentation = dateTimeOffset.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("es-MX")); // Noncompliant

        stringRepresentation = dateTimeOffset.ToString(CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = dateTimeOffset.ToString(CultureInfo.InvariantCulture);
        stringRepresentation = dateTimeOffset.ToString("d");
        stringRepresentation = dateTimeOffset.ToString("d", CultureInfo.GetCultureInfo("es-MX"));
    }

    public void DateOnlyCases(DateOnly dateOnly)
    {
        var stringRepresentation = dateOnly.ToString("dd/MM/yyyy HH:mm:ss"); // Noncompliant
        stringRepresentation = dateOnly.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("es-MX")); // Noncompliant

        stringRepresentation = dateOnly.ToString(CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = dateOnly.ToString(CultureInfo.InvariantCulture);
        stringRepresentation = dateOnly.ToString("d");
        stringRepresentation = dateOnly.ToString("f", CultureInfo.GetCultureInfo("es-MX"));
    }

    public void TimeOnlyCases(TimeOnly timeOnly)
    {
        var stringRepresentation = timeOnly.ToString("dd/MM/yyyy HH:mm:ss"); // Noncompliant
        stringRepresentation = timeOnly.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("es-MX")); // Noncompliant

        stringRepresentation = timeOnly.ToString(CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = timeOnly.ToString(CultureInfo.InvariantCulture);
        stringRepresentation = timeOnly.ToString("d");
        stringRepresentation = timeOnly.ToString("d", CultureInfo.GetCultureInfo("es-MX"));
    }

    public void ExoticCases()
    {
        var chainedInvocations = DateTime.UtcNow.AddDays(1).ToString("dd/MM/yyyy").EndsWith("1"); // Noncompliant
        (true ? new DateTime(1) : new DateTime(1)).ToString("dd/MM/yyyy"); // Noncompliant
        MyMethod().ToString("dd/MM/yyy");  // Noncompliant
        DateTime MyMethod() => new DateTime(1);
        new DateTime(1).ToString("dd/MM/yyy"); // Noncompliant
        MyDate.ToString("dd/MM/yyy");
    }

    static class MyDate
    {
        public static string ToString(string str) => str;
    }
}
