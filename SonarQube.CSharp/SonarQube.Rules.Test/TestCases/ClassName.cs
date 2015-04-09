namespace Tests.Diagnostics
{
    class foo // Noncompliant
    {
    }

    partial class Foo
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
