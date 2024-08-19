/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

namespace IntentionalFindings
{
    public static class S2251
    {
        public static void TestMethod()
        {
            const int limit = 5;
            var sum = 0;

            for (int i = 10; i > limit; i++)  // Noncompliant (S2251) {{'i' is incremented and will never reach 'stop condition'.}}
            {
                sum++;
            }
        }
    }
}
