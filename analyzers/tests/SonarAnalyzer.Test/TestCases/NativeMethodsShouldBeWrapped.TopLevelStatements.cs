using System.Runtime.InteropServices;

// Invalid method, testing if IMethodSymbol.ContainingType returns null (it currently doesn't)
private void Do(int x) { } // Error [CS0106]

public record Record
{
    extern private static void Extern0(); // Compliant

    extern private static void Extern1(string s, int x); // Compliant

    extern public static void Extern2(string s, int x); // Noncompliant {{Make this native method private and provide a wrapper.}}
    //                        ^^^^^^^

    public void Method()
    {
        extern static void Extern(string s, int x); // Compliant - local method
    }
}

namespace TestsFromDeprecatedS4214
{
    public record Record
    {
        public void Method()
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            static extern bool RemoveDirectory(string name);     // Compliant - Method is not publicly exposed
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool RemoveDirectory(string name);  // Noncompliant
    }
}

interface Foo
{
    [DllImport("mynativelib")]
    extern public static void Bar(string s, int x);     // FN
}
