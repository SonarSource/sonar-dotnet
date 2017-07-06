using System;
using System.Globalization;
using System.Resources;

namespace Tests.Diagnostics
{
    class Program
    {
        void InvalidCases()
        {
            42.ToString(); // Noncompliant {{Use the overload that takes a 'CultureInfo' or 'IFormatProvider' parameter.}}
//          ^^^^^^^^^^^^^
            string.Format("bla"); // Noncompliant
        }

        void ValidCases()
        {
            42.ToString(CultureInfo.CurrentCulture);
            string.Format(CultureInfo.CurrentUICulture, "{0}", 42);

            Activator.CreateInstance(); // Compliant - excluded

            var resourceManager = new ResourceManager(typeof(Program));
            resourceManager.GetObject("a"); // Compliant - excluded
            resourceManager.GetString("a"); // Compliant - excluded
        }
    }
}