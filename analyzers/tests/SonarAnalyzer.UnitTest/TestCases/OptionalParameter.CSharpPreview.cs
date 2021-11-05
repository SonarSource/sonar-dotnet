namespace Tests.Diagnostics
{
    public interface IInterface
    {
        static abstract void Method(int i = 42); //Noncompliant
//                                        ^^^^
    }

    public class Base : IInterface
    {
        public static void Method(int i = 42)
        {
        }

        public static void Method2(int i = 42) //Noncompliant {{Use the overloading mechanism instead of the optional parameters.}}
        {
        }
    }
}
