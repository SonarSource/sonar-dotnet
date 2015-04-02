namespace Tests.Diagnostics
{
    class foo // Noncompliant
    {
    }

    class Foo
    {
    }

    class I // Noncompliant
    {
    }

    class IFoo // Noncompliant
    {
    }

    class IdentityFoo
    {
    }

    partial class
    Foo 
    {
    }

    partial class
    IBar // Noncompliant
    {
    }
}
