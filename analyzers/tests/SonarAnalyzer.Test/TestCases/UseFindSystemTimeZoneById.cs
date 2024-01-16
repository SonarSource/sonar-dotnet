using System;
using TimeZoneConverter;

public class Program
{
    public void Compliant()
    {
        TZConvert.IanaToWindows("Asia/Tokyo");
        TZConvert.WindowsToIana("Asia/Tokyo");
        string resolvedTimeZone;
        TZConvert.TryIanaToWindows("Asia/Tokyo", out resolvedTimeZone);
        TZConvert.TryWindowsToIana("Asia/Tokyo", out resolvedTimeZone);

        TZConvert.IanaToRails("Asia/Tokyo");
        TZConvert.TryRailsToIana("Asia/Tokyo", out resolvedTimeZone);
    }
}
