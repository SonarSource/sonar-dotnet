using System;

namespace SomeNamespace
{
    class Program
    {
    }

    static class Helpers1
    {
        static void Foo(this Program p) // Noncompliant {{Either move this extension to another namespace or move the method inside the type itself.}}
//                  ^^^
        { }
    }
}

namespace SomeOtherNamespace
{
    static class Helpers2
    {
        static void Bar(this SomeNamespace.Program p) // Compliant
        {
        }
    }
}