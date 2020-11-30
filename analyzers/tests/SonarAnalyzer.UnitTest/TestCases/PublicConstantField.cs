namespace Tests.Diagnostics
{
    public class A
    {
        public const int A1 = 5; // Noncompliant {{Change this constant to a 'static' read-only property.}}
//                       ^^
        private const int B = 5;
        public int C = 5;
    }

    internal class b
    {
        public const int A = 5; // Compliant
        private const int B = 5;
    }
}
