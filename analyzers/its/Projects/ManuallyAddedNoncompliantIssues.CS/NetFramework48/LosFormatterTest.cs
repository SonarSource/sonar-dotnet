/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System.Web.UI;

namespace NetFramework48
{
    public class LosFormatterTest
    {
        public void Constructors()
        {
            new LosFormatter(); // Noncompliant (S5773) {{Serialized data signature (MAC) should be verified.}}

            new LosFormatter(false, ""); // Noncompliant (S5773) {{Serialized data signature (MAC) should be verified.}}
            new LosFormatter(false, new byte[0]); // Noncompliant (S5773) {{Serialized data signature (MAC) should be verified.}}

            new LosFormatter(true, ""); // Compliant - MAC filtering is enabled
            new LosFormatter(true, new byte[0]); // Compliant - MAC filtering is enabled
        }
    }
}
