namespace Tests.Diagnostics
{
    public interface IFoo
    {
        static abstract int GetValue();
    }

    public class Foo : IFoo
    {
        public static int GetValue() // Compliant - implements interface so cannot get rid of the method
        {
            return 42;
        }
    }
}
