using System;
using System.Globalization;

public class DateTimeFormatShouldNotBeHardcoded
{
    private string Format = $"dd/MM";

    public void Noncompliant(DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
    {
        var stringRepresentation = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"); // Noncompliant {{Do not hardcode the format specifier.}}
//                                                          ^^^^^^^^^^^^^^^^^^^^^
        stringRepresentation = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("es-MX")); // Noncompliant
        stringRepresentation = DateTime.Now.ToString(provider: CultureInfo.GetCultureInfo("es-MX"), format: "dd/MM/yyyy HH:mm:ss"); // Noncompliant
        stringRepresentation = DateTime.UtcNow.ToString(Format); // Noncompliant

        var stringFormat = string.Format("{0:yy/MM/dd}", DateTime.Now); // FN
        Console.WriteLine("{0:HH:mm}", DateTime.Now); // FN

        stringRepresentation = dateTimeOffset.ToString("dd/MM/yyyy HH:mm:ss"); // Noncompliant
        stringRepresentation = timeSpan.ToString(@"dd\.hh\:mm\:ss"); // Noncompliant

        var chainedInvocations = DateTime.UtcNow.AddDays(1).ToString("dd/MM/yyyy").EndsWith("1"); // Noncompliant
        (true ? new DateTime(1) : new DateTime(1)).ToString("dd/MM/yyyy"); // Noncompliant
        MyMethod().ToString("dd/MM/yyy");  // Noncompliant
        DateTime MyMethod() => new DateTime(1);
        new DateTime(1).ToString("dd/MM/yyy"); // Noncompliant
    }

    public void Compliant(DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
    {
        var stringRepresentation = DateTime.UtcNow.ToString(CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
        stringRepresentation = DateTime.UtcNow.ToString("d");
        stringRepresentation = DateTime.UtcNow.ToString("d", CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = dateTimeOffset.ToString(CultureInfo.GetCultureInfo("es-MX"));
        stringRepresentation = timeSpan.ToString("d");
        stringRepresentation = DateTime.UtcNow.ToString();

        MyDate.ToString("dd/MM/yyy");
    }

    static class MyDate
    {
        public static string ToString(string str) => str;
    }
}
