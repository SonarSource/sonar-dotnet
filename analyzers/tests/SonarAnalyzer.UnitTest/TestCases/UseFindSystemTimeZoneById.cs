using System;
using TimeZoneConverter;

public class Program
{
    public void Noncompliant()
    {
        TZConvert.IanaToWindows("Asia/Tokyo"); // Noncompliant {{Use "TimeZoneInfo.FindSystemTimeZoneById" instead of "TZConvert.IanaToWindows"}}
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
