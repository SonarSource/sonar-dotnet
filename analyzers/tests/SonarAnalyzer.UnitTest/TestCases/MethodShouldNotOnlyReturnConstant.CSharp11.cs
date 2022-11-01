namespace Tests.Diagnostics
{
    public interface IFoo
    {
        static abstract int GetValue();

        static virtual int GetAnotherValue() { return 42; } // Compliant - interface methods are ignored
    }

    public class Foo : IFoo
    {
        public static int GetValue() // Compliant - implements interface so cannot get rid of the method
        {
            return 42;
        }
    }
}
