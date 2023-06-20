using System;
using System.Globalization;

public class DateTimeFormatShouldNotBeHardcoded
{
    public void DateTimeCases()
    {
        var stringRepresentation = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"); // Noncompliant {{Use the CultureInfo class to provide the format for the string.}}
//                                                 ^^^^^^^^
        stringRepresentation = DateTime.UtcNow.ToString("dd/mm/yyyy HH:MM:ss"); // Noncompliant
        stringRepresentation = DateTime.UtcNow.ToString(CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
    }

    public void DateTimeOffsetCases(DateTimeOffset dateTimeOffset)
    {
        var stringRepresentation = dateTimeOffset.ToString("dd/MM/yyyy HH:mm:ss"); // Noncompliant
        stringRepresentation = dateTimeOffset.ToString("dd/mm/yyyy HH:MM:ss"); // Noncompliant
        stringRepresentation = dateTimeOffset.ToString(CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = dateTimeOffset.ToString(CultureInfo.InvariantCulture);
    }

    public void DateOnlyCases(DateOnly dateOnly)
    {
        var stringRepresentation = dateOnly.ToString("dd/MM/yyyy HH:mm:ss"); // Noncompliant
        stringRepresentation = dateOnly.ToString("dd/mm/yyyy HH:MM:ss"); // Noncompliant
        stringRepresentation = dateOnly.ToString(CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = dateOnly.ToString(CultureInfo.InvariantCulture);
    }

    public void TimeOnlyCases(TimeOnly timeOnly)
    {
        var asd = timeOnly.ToString("dd/MM/yyyy HH:mm:ss"); // Noncompliant
        asd = timeOnly.ToString("dd/mm/yyyy HH:MM:ss"); // Noncompliant
        asd = timeOnly.ToString(CultureInfo.GetCultureInfo("es-MX"));
        asd = timeOnly.ToString(CultureInfo.InvariantCulture);
    }
}
