using System;
using System.Globalization;
using System.Resources;

namespace Tests.Diagnostics
{
    static class Methods
    {
        public static int DoStuff(string foo) => 1;

        [Obsolete]
        public static int DoStuff(string foo, IFormatProvider format) => 1;


        public static int DoStuff2(string foo) => 1;

        [Obsolete]
        public static int DoStuff2(string foo, IFormatProvider format) => 1;
        public static int DoStuff2(string foo, CultureInfo foo2) => 1;
    }

    class Program
    {
        enum Colors { Red, Green, Blue, Yellow = 12 };

        void InvalidCases()
        {
            42.ToString(); // Noncompliant {{Use the overload that takes a 'CultureInfo' or 'IFormatProvider' parameter.}}
//          ^^^^^^^^^^^^^
            string.Format("bla"); // Noncompliant
            Methods.DoStuff2("foo"); // Noncompliant

            "".StartsWith(""); // Noncompliant

            Convert.ToInt32("123"); // Noncompliant
            int result;
            int.TryParse("123", out result); // Noncompliant, ToInt32(string, IFormatProvider)

            object number = 1.23;
            Convert.ToInt32(number); // Noncompliant, there is ToInt32(object, IFormatProvider)

            char.ToUpper('a'); // Noncompliant
            char.ToLower('a'); // Noncompliant

            "asdasd".ToUpper(); // Noncompliant
        }

        void ValidCases()
        {
            42.ToString(CultureInfo.CurrentCulture);
            string.Format(CultureInfo.CurrentUICulture, "{0}", 42);

            Activator.CreateInstance(); // Compliant - excluded // Error [CS0411] - cannot infer type

            var resourceManager = new ResourceManager(typeof(Program));
            resourceManager.GetObject("a"); // Compliant - excluded
            resourceManager.GetString("a"); // Compliant - excluded

            "".StartsWith("", StringComparison.CurrentCulture); // Compliant - StringComparison implies culture
            "".StartsWith("", true, CultureInfo.CurrentCulture); // Compliant

            Console.WriteLine("Colors.Red = {0}", Colors.Red.ToString("d"));
            Colors myColor = Colors.Yellow;
            Console.WriteLine("Colors.Red = {0}", myColor.ToString("d"));
            Methods.DoStuff("foo");

            Convert.ToInt32(1.23);
            Convert.ToInt32('1');
            Convert.ToChar(15);
        }
    }
}
