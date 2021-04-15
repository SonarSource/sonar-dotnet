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
            static void Foobar(this Program p) // Noncompliant // Error [CS1109] - extensions method can't be on inner classes
            {
            }
        }
    }

    static class Helpers1
    {
        static void Foo(this Program p) // Noncompliant {{Either move this extension to another namespace or move the method inside the class itself.}}
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

        static void Baz(this SomeNonExistingClass snec) // Error [CS0246] - unknown type
        {
        }
    }
}

namespace OtherPackageNamespace
{
    public interface IFoo { }

    public enum FooBar { }

    public struct FooQux { }

    public class Outer
    {
        public interface IBaz { }

        public enum BazBar { }

        public struct BazQux { }

    }

    public static class FooHelpers
    {
        public static void FooInterface(this IFoo foo) // Compliant
        {
        }

        public static void BazInterface(this Outer.IBaz foo) // Compliant
        {
        }

        public static void FooEnum(this FooBar foo) // Compliant
        {
        }

        public static void BazEnum(this Outer.BazBar foo) // Compliant
        {
        }

        public static void FooStruct(this FooQux foo) // Compliant
        {
        }

        public static void BazStruct(this Outer.BazQux foo) // Compliant
        {
        }

        public static T FooGeneric<T>(this T foo) // Compliant
        {
            return default(T);
        }
    }

}
