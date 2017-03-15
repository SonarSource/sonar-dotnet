using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class StringFormatArgumentNumberMismatch
    {
        void Test()
        {
            var s = "";
            s = string.Format("{0} {1} {2}", 1, 2, 3,
                4, 5); // Noncompliant {{The passed arguments do not match the format string.}}
//              ^^^^
            s = string.Format("{0}", 10,
                11); // Noncompliant; too many arguments
//              ^^
            s = string.Format("{0} {1} {2}", new[] { 1, 2 }); // Compliant, not recognized
            s = string.Format("{0} {1} {2}", new object[] { 1, 2 }); // Compliant, not recognized
            var pattern = "{0} {1} {2}";
            s = string.Format(pattern, 1, 2); // Compliant, not recognized

            const string pattern2 = "{0} {1} {2}";
            s = string.Format(pattern2, // Noncompliant
//                            ^^^^^^^^
                1, 2);
            s = string.Format(null, pattern2, 1, 2); // Noncompliant
            s = string.Format(null, pattern2, 1, 2, 3); // Compliant
            s = string.Format(null, pattern2, 1, 2, 3,
                4); // Noncompliant
            s = string.Format(null, "{3}", 1); // Noncompliant
            s = string.Format(null,
                "{3}", // Noncompliant
                1, 2, 3);
            s = string.Format(null, "{2}", 1, 2, 3); // Not recognized
            s = string.Format(null, "{}"); // not recognized
            s = string.Format(null, "{}", // not recognized
                1,2,3);
            s = string.Format(null, "{0}");  // Noncompliant
            s = string.Format(null, "{0}{2}{1}"); // Noncompliant
            s = string.Format(null, arg0: 1); // not recognized

            s = string.Format(null, "{2000}"); // not recognized

            s = string.Format(", 1 , 2, 3); // compliant
                ; ; ;
            s = string.Format("no format");
            s = string.Format("{0}", 1); // valid
            s = string.Format("{1}", "{0}", 1, 2); // Noncompliant
            int[] intArray = new int[] { };
            s = string.Format("{0}", intArray, intArray); // Noncompliant
        }
    }
}
