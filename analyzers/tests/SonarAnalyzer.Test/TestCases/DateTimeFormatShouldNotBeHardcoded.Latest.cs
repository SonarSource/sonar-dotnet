using System;
using System.Globalization;

public class DateTimeFormatShouldNotBeHardcoded
{
    public void Noncompliant(DateOnly dateOnly, TimeOnly timeOnly)
    {
        var stringRepresentation = dateOnly.ToString("dd/MM/yyyy"); // Noncompliant
        stringRepresentation = timeOnly.ToString("HH:mm:ss"); // Noncompliant
        stringRepresentation = timeOnly.ToString("HH:mm:ss\e"); // Noncompliant
    }

    public void Compliant(DateOnly dateOnly, TimeOnly timeOnly)
    {
        var stringRepresentation = dateOnly.ToString(CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = timeOnly.ToString(CultureInfo.GetCultureInfo("es-MX"));
    }
}
