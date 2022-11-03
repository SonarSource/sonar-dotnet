namespace Tests.Diagnostics
{
    public interface IFace
    {
        static virtual void Method1(int[,] a) { } //Noncompliant

        static abstract void Method2(int[,] a); //Noncompliant
    }

    public class PublicMethodWithMultidimensionalArray : IFace
    {
        public static void Method1(int[,] a) { } // Compliant

        public static void Method2(int[,] a) { } // Compliant
    }
}
