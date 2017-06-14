using System.Globalization;

namespace Tests.Diagnostics
{
    class Program
    {
        void Foo()
        {
            var s1 = "".ToLower(); // Noncompliant {{Change this normalization to 'String.ToUpper()'.}}
//                      ^^^^^^^
            s1 = "".ToLower(CultureInfo.InvariantCulture); // Noncompliant
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
