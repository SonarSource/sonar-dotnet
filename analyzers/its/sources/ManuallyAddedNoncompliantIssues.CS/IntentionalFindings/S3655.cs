/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

namespace IntentionalFindings
{
    public class S3655
    {
        public void ValueAccessOnEmptyNullable()
        {
            int? i = null;
            _ = i.Value; // Noncompliant (S3655) {{'i' is null on at least one execution path.}}
        }
    }
}
