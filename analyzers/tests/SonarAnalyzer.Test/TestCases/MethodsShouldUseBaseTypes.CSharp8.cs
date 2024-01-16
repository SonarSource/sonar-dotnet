using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Something
{
    internal interface IFoo
    {
        bool IsFoo { get; }
    }

    public class Foo : IFoo
    {
        public bool IsFoo { get; set; }
    }

    public class Bar : Foo
    {
    }
}

// Test that rule doesn't suggest base with inconsistent accessibility
namespace InconsistentAccessibility
{
    public class Bar
    {
        protected internal void ProtectedInternal_InternalBaseType(Something.Foo f) // Noncompliant
        {
            var x = f.IsFoo;
        }

        protected internal void ProtectedInternal_PublicBaseType(Something.Bar f) // Noncompliant
        {
            var x = f.IsFoo;
        }

        private protected void PrivateProtected_InternalBaseType(Something.Foo f) // Noncompliant
        {
            var x = f.IsFoo;
        }
    }

    public class Parent
    {
        private interface IPrivateFoo
        {
            bool IsFoo { get; }
        }

        public class ImplementsIPrivateFoo : IPrivateFoo
        {
            public bool IsFoo { get; }
        }

        private void Private(ImplementsIPrivateFoo foo) // Noncompliant
        {
            var x = foo.IsFoo;
        }

        private protected void PrivateProtected(ImplementsIPrivateFoo foo) // Compliant
        {
            var x = foo.IsFoo;
        }

        protected internal void ProtectedInternal(ImplementsIPrivateFoo foo) // Compliant
        {
            var x = foo.IsFoo;
        }

        private protected interface IPrivateProtectedFoo
        {
            bool IsFoo { get; }
        }

        public class ImplementsIPrivateProtectedFoo : IPrivateProtectedFoo
        {
            public bool IsFoo { get; }
        }

        private void Private(ImplementsIPrivateProtectedFoo foo) // Noncompliant
        {
            var x = foo.IsFoo;
        }

        private protected void PrivateProtected(ImplementsIPrivateProtectedFoo foo) // Noncompliant
        {
            var x = foo.IsFoo;
        }

        protected internal void ProtectedInternal(ImplementsIPrivateProtectedFoo foo) // Compliant
        {
            var x = foo.IsFoo;
        }

        protected internal interface IProtectedInternalFoo
        {
            bool IsFoo { get; }
        }

        public class ImplementsIProtectedInternalFoo : IProtectedInternalFoo
        {
            public bool IsFoo { get; }
        }

        private void Private(ImplementsIProtectedInternalFoo foo) // Noncompliant
        {
            var x = foo.IsFoo;
        }

        private protected void PrivateProtected(ImplementsIProtectedInternalFoo foo) // Noncompliant
        {
            var x = foo.IsFoo;
        }

        protected internal void ProtectedInternal(ImplementsIProtectedInternalFoo foo) // Noncompliant
        {
            var x = foo.IsFoo;
        }
    }
}
