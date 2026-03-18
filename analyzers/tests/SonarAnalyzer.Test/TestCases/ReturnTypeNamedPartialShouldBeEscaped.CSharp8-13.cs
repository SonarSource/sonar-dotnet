namespace ReturnTypeNamedPartial
{
    using System.Collections.Generic;

    public class Noncompliant
    {
        public delegate partial Delegate(); // Noncompliant {{Return types named 'partial' should be escaped with '@'}}
        //              ^^^^^^^

        public partial Method(int a) => null; // Noncompliant

        public ReturnTypeNamedPartial.partial Qualified() => null; // Noncompliant
        //                            ^^^^^^^

        global::ReturnTypeNamedPartial.partial Alias() => null; // Noncompliant
        //                             ^^^^^^^

        ref partial Refs(ref partial p) => ref p; // Noncompliant
        //  ^^^^^^^

        Parent.partial Nested() => null; // Noncompliant
        //     ^^^^^^^
    }

    public class Compliant
    {
        public delegate @partial Delegate();

        @partial Method() => null;

        public ReturnTypeNamedPartial.@partial Qualified() => null;

        global::ReturnTypeNamedPartial.@partial Alias() => null;

        ref @partial Refs(ref partial p) => ref p;

        unsafe partial* Pointer() => null;

        partial[] Array() => null;

        List<partial> List() => null;

        partial<int> Generic() => null;

        partial GenricMethod<T>() => null;

        public delegate partial GenericDelegate<T>();

        public ReturnTypeNamedPartial.partial Qualified<T>() => null;

        global::ReturnTypeNamedPartial.partial GenericAlias<T>() => null;

        ref partial GenericRefs<T>(ref partial p) => ref p;

        void GenericLocalFunction()
        {
            partial LocalFunction<T>() => null;
        }

        partial.Nested Nested() => null;

        Parent.partial Nested<T>() => null;

        Parent.@partial NestedEscaped() => null;

        partial? Nullable() => null;

        void VoidMethod() { }

        (int, int) TupleMethod() { return (1, 0); }
    }

    public class partial
    {
        public class Nested { }
    }

    public class partial<T> { }

    public class Parent
    {
        public class partial { }
    }
}

namespace Alias
{
    using partial = System.Int32;
    using System.Collections.Generic;

    using System.Collections.Generic;

    public class Noncompliant
    {
        public delegate partial Delegate(); // Noncompliant
        //              ^^^^^^^

        public partial Method(int a) => default; // Noncompliant

        public ReturnTypeNamedPartial.partial Qualified() => default; // Noncompliant
        //                            ^^^^^^^

        global::ReturnTypeNamedPartial.partial Alias() => default; // Noncompliant
        //                             ^^^^^^^

        ref partial Refs(ref partial p) => ref p; // Noncompliant
        //  ^^^^^^^
    }

    public class Compliant
    {
        public delegate @partial Delegate();

        @partial Method() => default;

        public ReturnTypeNamedPartial.@partial Qualified() => default;

        global::ReturnTypeNamedPartial.@partial Alias() => default;

        ref @partial Refs(ref partial p) => ref p;

        unsafe partial* Pointer() => default;

        partial[] Array() => default;

        List<partial> List() => default;

        partial GenricMethod<T>() => default;

        public delegate partial GenericDelegate<T>();

        public ReturnTypeNamedPartial.partial Qualified<T>() => default;

        global::ReturnTypeNamedPartial.partial GenericAlias<T>() => default;

        ref partial GenericRefs<T>(ref partial p) => ref p;

        void GenericLocalFunction()
        {
            partial LocalFunction<T>() => default;
        }

        partial? Nullable() => null;

        (partial, partial) TupleMethod() => default;
    }
}

namespace GenericInterface
{
    interface INoncompliant<partial>
    {
        partial Method(); //Noncompliant
    }

    interface ICompliant<partial>
    {
        @partial Method();

        partial Method<T>() => default;
    }
}

namespace partial
{

    class Compliant
    {
        partial.Compliant Method() => null;

        global::partial.Compliant Method2() => null;
    }
}
