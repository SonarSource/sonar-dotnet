using System;

namespace Tests.Diagnostics
{
    interface IFoo { }
    class FooBase : IFoo { }
    class Foo1 : FooBase { }

    class FieldClass // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 3 to the maximum authorized 1 or less.}}
    {
        private IFoo foo = new FooBase();
        private FooBase foo2 = GetFoo();
        private IFoo foo3 = Foo;
        private int i; // Primitives don't count

        private static FooBase GetFoo() => null;

        private static Foo1 Foo { get; }
    }

    class BasePropertyClass
    {
        public IFoo Foo { get; set; }

        public IFoo Foo2
        {
            get
            {
                return new FooBase();
            }
        }

        private static Foo1 GetFoo() => null;
    }
}
