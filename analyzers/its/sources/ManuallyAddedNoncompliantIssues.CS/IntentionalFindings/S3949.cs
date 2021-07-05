/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

namespace IntentionalFindings
{
    public class S3949
    {
        public void PositiveOverflow() // Noncompliant (S2325)
        {
            int i = 2147483600;
            i += 100; // Disabled rule (S3949) {{There is a path on which this operation always overflows}}
        }

        public void NegativeOverflow() // Noncompliant
        {
            int i = -2147483600;
            i -= 100; // Disabled rule (S3949) {{There is a path on which this operation always overflows}}
        }
    }
}
