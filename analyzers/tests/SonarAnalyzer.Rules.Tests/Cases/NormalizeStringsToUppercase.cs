using System.Globalization;

namespace Tests.Diagnostics
{
    class Program
    {
        void StringLowerCalls()
        {
            var s1 = "".ToLower(); // Compliant
            s1 = "".ToLower(CultureInfo.CurrentCulture);


            s1 = "".ToLower(CultureInfo.InvariantCulture); // Noncompliant  {{Change this normalization to 'ToUpperInvariant()'.}}
//                  ^^^^^^^
            s1 = "".ToLowerInvariant(); // Noncompliant

            s1 = Ext.ToLower("", 42);
            s1 = "".ToLower(42);
        }

        void CharLowerCalls()
        {
            var s1 = char.ToLower('a'); // Compliant
            s1 = char.ToLower('a', CultureInfo.CurrentCulture);


            s1 = char.ToLower('a', CultureInfo.InvariantCulture); // Noncompliant  {{Change this normalization to 'ToUpperInvariant()'.}}
//                    ^^^^^^^
            s1 = char.ToLowerInvariant('a'); // Noncompliant

            s1 = Ext.ToLower('a', 42);
            s1 = 'a'.ToLower(42);
        }
    }

    static class Ext
    {
        public static string ToLower(this string s, int i) => s;
        public static char ToLower(this char s, int i) => s;
    }
}
