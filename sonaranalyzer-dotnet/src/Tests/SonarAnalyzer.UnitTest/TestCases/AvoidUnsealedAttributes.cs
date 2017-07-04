using System;

namespace Tests.Diagnostics
{
    public class MyAttribute : Attribute // Noncompliant {{Seal this attribute or make it abstract.}}
//               ^^^^^^^^^^^
    {
    }

    protected class MyOtherAttribute : Attribute // Noncompliant
    {
    }

    protected internal class MyOtherAttribute2 : Attribute // Noncompliant
    {
    }

    public sealed class Bar : Attribute // Compliant - sealed
    { }

    public abstract class FooBar : Attribute // Compliant - abstract
    {
    }

    public sealed class Attr : Attribute
    {
        private class InnerAttr : Attribute // Compliant - private
        {
            public class InnerInnerAttr : Attribute // Compliant - effective accessibility is private
            {
            }
        }
    }
}
