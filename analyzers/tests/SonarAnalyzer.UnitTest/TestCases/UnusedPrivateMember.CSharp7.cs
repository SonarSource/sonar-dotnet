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

    // https://github.com/SonarSource/sonar-dotnet/issues/2478
    public class ReproIssue2478
    {
        public void SomeMethod()
        {
            var (a, (barA, barB)) = new PublicDeconstructWithInnerType();

            var (_, _, c) = new PublicDeconstruct();

            var qix = new MultipleDeconstructors();
            object b;
            (a, b, c) = qix;

            (a, b) = ReturnFromMethod();

            (a, b) = new ProtectedInternalDeconstruct();

            (a, b, c) = new Ambiguous(); // Error [CS0121]
        }

        internal void InternalMethod(InternalDeconstruct bar)
        {
            var (a, b) = bar;
        }

        private sealed class PublicDeconstructWithInnerType
        {
            public void Deconstruct(out object a, out InternalDeconstruct b) { a = b = null; }

            // deconstructors must be public, internal or protected internal
            private void Deconstruct(out object a, out object b) { a = b = null; } // Noncompliant
        }

        internal sealed class InternalDeconstruct
        {
            internal void Deconstruct(out object a, out object b) { a = b = null; }
        }

        private class PublicDeconstruct
        {
            public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; }

            // deconstructors must be public, internal or protected internal
            protected void Deconstruct(out string a, out string b, out string c) { a = b = c = null; } // Noncompliant
        }

        private sealed class MultipleDeconstructors
        {
            public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; }

            public void Deconstruct(out object a, out object b) // Noncompliant
            {
                a = b = null;
            }
        }

        internal class ProtectedInternalDeconstruct
        {
            protected internal void Deconstruct(out object a, out object b) { a = b = null; }
        }

        private class Ambiguous
        {
            public void Deconstruct(out string a, out string b, out string c) { a = b = c = null; }
            public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; } // Noncompliant FP, actually the one above is not used
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
