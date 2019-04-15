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

        public static int DoStuff3(string foo) => 1;
        public static int DoStuff3(int foo, IFormatProvider format) => 1;

        public static int DoStuff4(string foo, string bar, string quix) => 1;
        public static int DoStuff4(string foo, string bar, int quix, IFormatProvider format) => 1;

        public static int DoStuff5(string foo, string bar, string quix) => 1;
        public static int DoStuff5(string foo, string bar, string quix, IFormatProvider format) => 1;

        public static int DoStuff6(string foo, string bar, string quix) => 1;
        public static int DoStuff6(string foo, CultureInfo cultureInfo, string bar, string quix) => 1;

        public static int DoStuff7(string foo, string bar, string quix) => 1;
        public static int DoStuff7(CultureInfo cultureInfo, string foo, string bar, string quix) => 1;

        public static int DoStuff8(string foo, string bar, string quix) => 1;
        public static int DoStuff8(CultureInfo cultureInfo, string foo, IFormatProvider formatProvider, string bar, string quix) => 1;

        public static int DoStuff9(string foo, string bar, string quix) => 1;
        public static int DoStuff9(CultureInfo cultureInfo, IFormatProvider formatProvider, string bar, string quix) => 1;

        public static bool DoStuff10(string foo, bool @default = false) => true;
        public static bool DoStuff10(string foo) => true;
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

            "".StartsWith(""); // FN, overload with CultureInfo has 3 parameters and is difficult to tell if is the proper overload

            Convert.ToInt32("123"); // Noncompliant
            int result;
            int.TryParse("123", out result); // FN, TryParse(string, NumberStyles, IFormatProvider, out result) is difficult to tell if is real overload

            object number = 1.23;
            Convert.ToInt32(number); // Noncompliant, there is ToInt32(object, IFormatProvider)

            char.ToUpper('a'); // Noncompliant
            char.ToLower('a'); // Noncompliant

            "asdasd".ToUpper(); // Noncompliant

            String.Format("%s %s", "foo", "bar", "quix", "hi", "bye"); // Noncompliant
            String.Format("%s %s", "foo"); // FN, the signature is not params for this one
            String.Format("%s %s", "foo", "bar"); // FN, the signature is not params for this one
            String.Format("%s %s", "foo", "bar", "quix"); // FN, the signature is not params for this one

            Methods.DoStuff5("foo", "bar", "qix"); // Noncompliant
            Methods.DoStuff6("foo", "bar", "qix"); // Noncompliant
            Methods.DoStuff7("foo", "bar", "qix"); // Noncompliant
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

            Methods.DoStuff3("foo"); // Compliant - the other DoStuff3 does not have the same signature

            Methods.DoStuff4("foo", "", ""); // Compliant - the other DoStuff4 does not have the same signature

            Methods.DoStuff5("foo", "bar", "qix", new MyFormat());
            Methods.DoStuff6("foo", CultureInfo.CurrentCulture, "bar", "qix");
            Methods.DoStuff7(CultureInfo.DefaultThreadCurrentCulture, "foo", "bar", "qix");

            Methods.DoStuff8("foo", "bar", "qix"); // Compliant, alternative has too many params
            Methods.DoStuff9("foo", "bar", "qix"); // Compliant, alternative is not overload

            Methods.DoStuff10("foo"); // Compliant
        }

        class MyFormat : IFormatProvider
        {
            public object GetFormat(Type formatType) => null;
        }
    }
}
