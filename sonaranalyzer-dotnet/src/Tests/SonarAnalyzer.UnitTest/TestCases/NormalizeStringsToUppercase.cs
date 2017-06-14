using System.Globalization;

namespace Tests.Diagnostics
{
    class Program
    {
        void Foo()
        {
            var s1 = "".ToLower(); // Compliant
            s1 = "".ToLower(CultureInfo.CurrentCulture);


            s1 = "".ToLower(CultureInfo.InvariantCulture); // Noncompliant  {{Change this normalization to 'String.ToUpperInvariant()'.}}
//                  ^^^^^^^
            s1 = "".ToLowerInvariant(); // Noncompliant

            s1 = Ext.ToLower("", 42);
            s1 = "".ToLower(42);
        }
    }

    static class Ext
    {
        public static string ToLower(this string s, int i) => s;
    }
}
