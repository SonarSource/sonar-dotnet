using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class IfCollapsible
    {
        public void Test(bool cond1, bool cond2, bool cond3)
        {
            while (cond1)
            {
                if (cond2 || cond3)
                {
                }
            }

            if (cond1)
//          ^^ Secondary [0]
            {
                if (cond2 || cond3)
//              ^^ Noncompliant [0]
                {
                }
            }

            if (cond1) // Secondary [1] {{Merge this if statement with its nested one.}}
                if (cond2 || cond3) // Noncompliant [1] {{Merge this if statement with the enclosing one.}}
                {
                }

            if (cond1)
            {
                if (cond2 || cond3)
                {
                }
                else
                {
                }
            }

            if (cond1)
            {
                var x = 5;
                if (cond2 || cond3)
                {
                }
            }

            if (cond1 && (cond2 || cond3))
            {
            }

            if (cond1)
            {
                if (cond2 || cond3) // Compliant, parent has else
                {
                }
            }
            else
            {
            }
        }
    }

    // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/9221
    public class Repro_9221
    {
        public DateTime? ThisWorks()
        {
            dynamic anything = "2024-05-13";
            object anythingObj = "2024-05-13";
            string notDynamic = "2024-05-13";

            if (anything is string)
                if (DateTime.TryParseExact(anything, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)) // Compliant - Contains dynamic identifier
                    return dt;

            if (anythingObj is string) // Secondary
                if (DateTime.TryParseExact((string)anythingObj, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt1)) // Noncompliant
                    return dt1;

            if (anythingObj is string && DateTime.TryParseExact((string)anythingObj, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt2))
                    return dt2;

            if (notDynamic != null)
                if (notDynamic == "something" && anything is string) // Compliant - Contains dynamic identifier
                    return null;

            if (notDynamic != null)
                if (notDynamic == (anything is string ? notDynamic : string.Empty) && notDynamic is string) // Compliant - Contains dynamic identifier
                    return null;

            if (notDynamic != null) // Secondary
                if (notDynamic == "something") // Noncompliant
                    return null;

            return null;
        }

        public DateTime? ThisWontWork()
        {
            dynamic anything = "2024-04-29";

            if (anything is string && DateTime.TryParseExact(anything, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                return dt; // Error [CS0165]

            return null;
        }
    }
}
