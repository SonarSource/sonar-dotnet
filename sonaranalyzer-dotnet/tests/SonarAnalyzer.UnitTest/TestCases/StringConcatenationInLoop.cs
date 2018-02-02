using System.Collections.Generic;
using System.IO;

namespace Tests.Diagnostics
{
    public class StringConcatenationInLoop
    {
        public StringConcatenationInLoop()
        {
            string s = "";
            for (int i = 0; i < 50; i++)
            {
                var sLoop = "";

                s = s + "a" + "b";  // Noncompliant
//              ^^^^^^^^^^^^^^^^^
                s += "a";     // Noncompliant {{Use a StringBuilder instead.}}
                sLoop += "a"; // Compliant

                i += 5;
            }
            s += "a";

            while (true)
            {
                // See https://github.com/SonarSource/sonar-csharp/issues/1138
                s = s ?? "b";
            }
        }
    }
}
