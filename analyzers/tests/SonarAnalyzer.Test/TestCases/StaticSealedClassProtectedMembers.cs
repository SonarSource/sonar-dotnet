using System;

namespace Tests.Diagnostics
{
    sealed class Class1
    {
        public int field0; // Compliant

        protected int field1; // Noncompliant {{Remove this 'protected' modifier.}}
//      ^^^^^^^^^

        internal protected int field2; // Noncompliant
//               ^^^^^^^^^

        internal readonly protected int field3; // Noncompliant
//                        ^^^^^^^^^

        protected static int field4; // Noncompliant
//      ^^^^^^^^^

        internal protected static int field5; // Noncompliant
//               ^^^^^^^^^

        internal readonly protected static int field6; // Noncompliant
//                        ^^^^^^^^^

        protected const int const1 = 5; // Noncompliant
//      ^^^^^^^^^

        internal protected const int const2 = 10; // Noncompliant
//               ^^^^^^^^^

        private Class1(int counter, string name) // Compliant
        { }
        
        public Class1(int counter) // Compliant
        { }

        internal Class1(long counter) // Compliant
        { }

        protected Class1() // Noncompliant
//      ^^^^^^^^^
        { }

        internal protected Class1(string name) // Noncompliant
//               ^^^^^^^^^
        { }

        protected void Test1() // Noncompliant
//      ^^^^^^^^^
        { }


        protected internal void Test2() // Noncompliant
//      ^^^^^^^^^
        { }

        private void Test3() // Compliant
        { }
        
        protected int MyProperty { get; set; } // Noncompliant
//      ^^^^^^^^^

        protected int this[int counter] // Noncompliant
//      ^^^^^^^^^
        {
            get { return 0; }
            set { }
        }

        protected event EventHandler<EventArgs> MyEvent // Noncompliant
//      ^^^^^^^^^
        {
            add { }
            remove { }
        }
    }

    public class Base1
    {
        protected virtual void Method1() // Compliant
        { }

        protected virtual int MyProperty { get; } // Compliant
    }

    public sealed class Subclass1 : Base1
    {
        protected override void Method1() // Compliant
        { }

        protected override int MyProperty // Compliant
        {
            get { return 0; }
        }
    }
}
