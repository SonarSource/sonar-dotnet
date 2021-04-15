public record Record
{
    extern private static void Extern0(); // Compliant

    extern private static void Extern1(string s, int x); // Compliant
    
    extern public static void Extern2(string s, int x); // Noncompliant {{Make this native method private and provide a wrapper.}}
//                            ^^^^^^^

    public void Method()
    {
        extern static void Extern(string s, int x); // Compliant - local method
    }
}
