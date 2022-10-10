namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        static abstract void foo(); // Noncompliant {{Rename method 'foo' to match pascal case naming rules, consider using 'Foo'.}}
//                           ^^^
        static abstract void Foo();
    }

    public record FooRecord : IMyInterface
    {
        static void IMyInterface.foo() { } // Compliant, we can't change it
        static void IMyInterface.Foo() { }
    }
}
