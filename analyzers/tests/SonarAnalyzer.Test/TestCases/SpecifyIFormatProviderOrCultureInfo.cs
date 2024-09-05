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

        public static bool DoStuff11(string foo, params string[] args) => true;
        public static bool DoStuff11(string foo, int bar, params string[] args) => true;

        public static bool DoStuff12(string foo, IFormatProvider provider, int x, params string[] args) => true;
        public static bool DoStuff12(string foo, params string[] args) => true;

        // the following test mimicks the behavior of System.String.Format(), which in some .NET distributions has an override with
        // an IFormatProvider parameter only for the method with 'params' argument
        public static string MyFormat(IFormatProvider provider, String format, params object[] args) => "";
        public static string MyFormat(String format, params object[] args) => "";
        public static string MyFormat(String format, object arg0) => "";
        public static string MyFormat(String format, object arg0, object arg1) => "";

        public static string MyFormat2(IFormatProvider provider, string format, bool boolCheck, params string[] args) => "";
        public static string MyFormat2(string format, bool boolCheck, string arg0) => "";
        public static string MyFormat2(string format, string arg0) => "";
        public static string MyFormat2(string format, bool boolCheck, Program arg0) => "";
        public static string MyFormat2(string format, Program program, params string[] args) => "";

        public static string DoStuff13(CultureInfo cultureInfo, params object[] args) => "";
        public static string DoStuff13(Program program) => "";

        public static string DoStuff14(CultureInfo cultureInfo) => "";
        public static string DoStuff14(Program program) => "";
    }

    class Program
    {
        enum Colors { Red, Green, Blue, Yellow = 12 };

        void InvalidCases()
        {
            42.ToString(); // Noncompliant {{Use the overload that takes a 'CultureInfo' or 'IFormatProvider' parameter.}}
//          ^^^^^^^^^^^^^
            Methods.DoStuff2("foo"); // Noncompliant

            "".StartsWith(""); // FN, overload with CultureInfo has 3 parameters and is difficult to tell if is the proper overload

            Convert.ToInt32("123"); // Noncompliant
            object number = 1.23;
            Convert.ToInt32(number); // Noncompliant, there is ToInt32(object, IFormatProvider)

            char.ToUpper('a'); // Noncompliant
            char.ToLower('a'); // Noncompliant

            "asdasd".ToUpper(); // Noncompliant

            string.Format("%s %s", "foo"); // Noncompliant
            string.Format("%s %s", "foo", "bar"); // Noncompliant
            string.Format("%s %s", "foo", "bar", "quix"); // Noncompliant
            string.Format("{0}", "foo"); // Noncompliant
            string.Format("{0}", new MyObject()); // Noncompliant
            string.Format("TimeClient {0}", 1); // Noncompliant

            Methods.DoStuff5("foo", "bar", "qix"); // Noncompliant
            Methods.DoStuff6("foo", "bar", "qix"); // Noncompliant
            Methods.DoStuff7("foo", "bar", "qix"); // Noncompliant

            Methods.MyFormat("%s", "foo"); // Noncompliant
            Methods.MyFormat("%s", "foo", "bar"); // Noncompliant
            Methods.MyFormat("%s", "foo", "bar", "qix"); // Noncompliant

            Methods.MyFormat2("%s", true, "x"); // Noncompliant

            Methods.DoStuff13(this); // Noncompliant
        }

        void ValidCases(MyFormat myFormat)
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

            Methods.DoStuff5("foo", "bar", "qix", myFormat);
            Methods.DoStuff6("foo", CultureInfo.CurrentCulture, "bar", "qix");
            Methods.DoStuff7(CultureInfo.DefaultThreadCurrentCulture, "foo", "bar", "qix");

            Methods.DoStuff8("foo", "bar", "qix"); // Compliant, alternative has too many params
            Methods.DoStuff9("foo", "bar", "qix"); // Compliant, alternative is not overload

            Methods.DoStuff10("foo"); // Compliant
            Methods.DoStuff11("foo"); // Compliant
            Methods.DoStuff12("foo"); // Compliant

            Methods.MyFormat(myFormat, "%s", "foo"); // Compliant
            Methods.MyFormat(myFormat, "%s", "foo", "bar"); // Compliant
            Methods.MyFormat(myFormat, "%s", "foo", "bar", "qix"); // Compliant

            Methods.MyFormat2(myFormat, "%s", true, "x"); // Compliant
            Methods.MyFormat2("%s", "bar"); // Compliant, there's no overload for it
            Methods.MyFormat2("%s", true, this); // Compliant, no overload
            Methods.MyFormat2("%s", this, "foo", "bar", "qix"); // Compliant, no overload

            Methods.DoStuff13(CultureInfo.CurrentCulture, this);

            Methods.DoStuff14(this); // Compliant, no alternative
        }

        class MyFormat : IFormatProvider
        {
            public object GetFormat(Type formatType) => null;
        }

        class MyObject { }
    }
}
