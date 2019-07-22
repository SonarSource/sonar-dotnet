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
            var (a, b) = new SomeDeconstructibleType();
        }

        private sealed class SomeDeconstructibleType
        {
            public void Deconstruct(out object a, out object b) // Noncompliant FP #2478
            {
                a = b = null;
            }
        }
    }

    public class ReproIssue2333
    {
        public void M()
        {
            OtherClass oc = new OtherClass();
            (oc.Name, oc.Type) = ("A", "B");
            Console.WriteLine(oc.Name + " " + oc.Type);
        }

        class OtherClass
        {
            public string Name { get; set; } // Noncompliant FP #2333
            public string Type { get; set; } // Noncompliant FP #2333
        }
    }

    public class EmptyCtor
    {
        // That's invalid syntax, but it is still empty ctor and we should not raise for it, even if it is not used
        public EmptyCtor() => // Error [CS1525,CS1002]
    }
}
