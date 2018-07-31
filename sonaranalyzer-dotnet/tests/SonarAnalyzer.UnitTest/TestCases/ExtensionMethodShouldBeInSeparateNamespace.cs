using System;

class GlobalClass
{
}

static class GlobalNamespaceClass
{
    static void Qux(this GlobalClass i) // Noncompliant
    {
    }

    static void Quux(this SomeNonExistingClass snec) // ErrorType is considered as part of global namespace but we don't want to report on it
    {
    }
}

namespace SomeNamespace
{
    class Program
    {
        static class SubClass
        {
            // Doesn't compile: CS1109 extensions method can't be on inner classes
            static void Foobar(this Program p) // Noncompliant
            {
            }
        }
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

        static void Baz(this SomeNonExistingClass snec)
        {
        }
    }
}

namespace OtherPackageNamespace
{
    public interface IFoo { }

    public enum FooBar { }

    public struct FooQux { }

    public static class FooHelpers
    {
        public static void FooInterface(this IFoo foo) // Compliant
        {
        }

        public static void FooEnum(this FooBar foo) // Compliant
        {
        }

        public static void FooStruct(this FooQux foo) // Compliant
        {
        }

        public static T FooGeneric<T>(this T foo) // Compliant
        {
            return default(T);
        }
    }

}
