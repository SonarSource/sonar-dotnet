using System.Runtime.InteropServices;

interface Foo
{
    [DllImport("mynativelib")]
    extern public static void Bar(string s, int x); // Compliant, FN
}
