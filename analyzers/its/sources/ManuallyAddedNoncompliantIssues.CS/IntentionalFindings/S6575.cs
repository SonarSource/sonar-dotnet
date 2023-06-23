/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using TimeZoneConverter;

namespace IntentionalFindings
{
    public class S6575
    {
        public void ConvertTimeZone() =>
            TZConvert.IanaToWindows("Asia/Tokyo"); // Noncompliant (S6575)
    }
}
