using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class ExpressionBodyProperties
    {
        private int field;

        private int Property01
        {
            get => field;
            set => field = value; // Noncompliant
        }

        private int Property02
        {
            get => field; // Noncompliant
            set => field = value;
        }

        public void Method()
        {
            int x;

            x = Property01;
            Property02 = x;
        }
    }

    public class ReproIssue2478
    {
        public void SomeMethod()
        {
            var (a, b) = new Foo();

            var (_, _, c) = new Baz();

            var qix = new Qix();
            (a, b, c) = qix;

            var (a, b) = ReturnFromMethod();
        }

        internal void InternalMethod(Bar bar)
        {
            var (a, b) = bar;
        }

        private sealed class Foo
        {
            public void Deconstruct(out object a, out object b) { a = b = null; }
        }

        internal sealed class Bar
        {
            internal void Deconstruct(out object a, out object b) { a = b = null; }
        }

        private sealed class Baz
        {
            public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; }
        }

        private sealed class Qix
        {
            public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; }

            public void Deconstruct(out object a, out object b) // Noncompliant
            {
                a = b = null;
            }
        }

        private ForMethod ReturnFromMethod() => null;
        private sealed class ForMethod
        {
            public void Deconstruct(out object a, out object b) { a = b = null; }
        }
    }

    public class ReproIssue2333
    {
        public void Method()
        {
            PrivateNestedClass x = new PrivateNestedClass();
            (x.ReadAndWrite, x.OnlyWriteNoBody, x.OnlyWrite) = ("A", "B", "C");
            var tuple = (x.ReadAndWrite, x.OnlyRead);
        }

        private class PrivateNestedClass
        {
            private string hasOnlyWrite;

            public string ReadAndWrite { get; set; }        // Setters are compliant, they are used in tuple assignment
            public string OnlyWriteNoBody { get; set; }     // Compliant, we don't raise on get without body

            public string OnlyRead
            {
                get;
                set;    // Noncompliant
            }

            public string OnlyWrite
            {
                get => hasOnlyWrite;    // Noncompliant
                set => hasOnlyWrite = value;
            }
        }
    }

    public class EmptyCtor
    {
        // That's invalid syntax, but it is still empty ctor and we should not raise for it, even if it is not used
        public EmptyCtor() => // Error [CS1525,CS1002]
    }
}
