/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System.Data.SqlClient; // will generate S1128

namespace IntentionalFindings
{
    public static class S2857
    {
        public static readonly string VALUE = "SELECT one, two" +
            "FROM table"; // Noncompliant (S2857) {{Add a space before 'FROM'.}}
    }
}
