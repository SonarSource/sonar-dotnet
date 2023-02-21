[assembly: System.Reflection.AssemblyVersion("1.0.0")]

namespace CSLib.foo
{
    public class IFoo
    {
        public int Prop => 42;
    }

    class IBar // NOSONAR
    {
    }
}
