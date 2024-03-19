/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */
 
namespace IntentionalFindings
{
    public static class S2166
    {
        public class CustomException { } // Noncompliant (S2166)
    }
}
