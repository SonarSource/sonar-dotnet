using TimeZoneConverter;

namespace IntentionalFindings
{
    public class S6575
    {
        public void ConvertTimeZone() =>
            TZConvert.IanaToWindows("Asia/Tokyo"); // Noncompliant (S6575)
    }
}
