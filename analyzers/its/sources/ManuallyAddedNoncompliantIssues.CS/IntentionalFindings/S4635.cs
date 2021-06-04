/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System;

namespace IntentionalFindings
{
    public static class S4635
    {
        public static int TestMethod() =>
            "Test".Substring(1).IndexOf('t', StringComparison.InvariantCulture); // Noncompliant (S4635) {{Replace 'IndexOf' with the overload that accepts an offset parameter.}}
    }
}
