using System;
using TimeZoneConverter;

public class Program
{
    public void Noncompliant()
    {
        TZConvert.IanaToWindows("Asia/Tokyo"); // Noncompliant {{Use "TimeZoneInfo.FindSystemTimeZoneById" directly instead of "TZConvert.IanaToWindows"}}
//                ^^^^^^^^^^^^^
        TZConvert.WindowsToIana("Asia/Tokyo"); // Noncompliant
        string resolvedTimeZone;
        TZConvert.TryIanaToWindows("Asia/Tokyo", out resolvedTimeZone); // Noncompliant
        TZConvert.TryWindowsToIana("Asia/Tokyo", out resolvedTimeZone); // Noncompliant
    }

    public void Compliant()
    {
        TZConvert.IanaToRails("Asia/Tokyo");
        string resolvedTimeZone;
        TZConvert.TryRailsToIana("Asia/Tokyo", out resolvedTimeZone);
    }
}

public class OtherTZConverterUsage
{

    private class TZConvert
    {
        public static string IanaToWindows(string ianaTimeZoneName)
        {
            return string.Empty;
        }
    }

    private void Compliant()
    {
        TZConvert.IanaToWindows("Asia/Tokyo");
    }
}
