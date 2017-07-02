using System;

namespace Tests.Diagnostics
{
    public class MyAttribute : Attribute // Noncompliant {{Seal this attribute or make it abstract.}}
//               ^^^^^^^^^^^
    {
    }

    private class Foo : Attribute // Compliant - private
    { }

    public sealed class Bar : Attribute // Compliant - sealed
    { }

    public abstract class FooBar : Attribute // Compliant - abstract
    {
    }
}
