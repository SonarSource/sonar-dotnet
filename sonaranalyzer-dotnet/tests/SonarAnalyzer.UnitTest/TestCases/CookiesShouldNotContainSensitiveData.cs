using System;
using System.Web;

namespace Tests.Diagnostics
{
    /// The general cases such as setting fields and properties and returning from methods
    /// are covered by the tests in <see cref="DoNotUseNonHttpCookies" /> and
    /// <see cref="DoNotUseInsecureCookies" />
    class Program
    {
        public static void Main()
        {
            var c = new HttpCookie("c"); // Compliant
            c["key"] = "value"; // Noncompliant

            var d = new HttpCookie("c", "value"); // Noncompliant

            var e = new HttpCookie("c", null); // Noncompliant

            var f = new HttpCookie("c", "value"); // Noncompliant
            f["key"] = null; // Noncompliant, could perhaps be considered as FP

            var g = new HttpCookie("c");
            g.Value = "value"; // Noncompliant

            g.Values.Add("key", "value"); // Compliant, FN - we cannot easily detect adding collection items
        }
    }
}
