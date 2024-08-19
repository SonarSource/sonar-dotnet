/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

namespace IntentionalFindings
{
    public class S4487
    {
        private int value; // Noncompliant (S4487) {{Remove this unread private field 'value' or refactor the code to use its value.}}

        public S4487()
        {
            value = 0;
        }

        public void SetField(int value)
        {
            this.value = value;
        }
    }

}
