using System;

namespace Tests.Diagnostics
{
    class StaticFieldWrittenFromInstanceMember
    {
        private static int count = 0;
//                         ^^^^^^^^^ Secondary [0]
//                         ^^^^^^^^^ Secondary@-1 [1]
        private int countInstance = 0;

        private static string text; // Secondary

        public void DoSomething()
        {
            count++;  // Noncompliant [0] {{Make the enclosing instance method 'static' or remove this set on the 'static' field.}}
//          ^^^^^
            var action = new Action(() =>
            {
                count++; // Compliant, already reported on this symbol
            });
            countInstance++;

            text ??= "empty"; // Noncompliant
        }

        public static void DoSomethingStatic()
        {
            count++;
            text ??= "empty";
        }

        public int MyProperty
        {
            get { return myVar; }
            set
            {
                count++; // Noncompliant [1] {{Make the enclosing instance property 'static' or remove this set on the 'static' field.}}
                count += 42; // Compliant, already reported on this symbol
                myVar = value;
            }
        }

        private int myVar;

        public int MyProperty2
        {
            get { return myVar; }
            set { myVar = value; }
        }
    }

    interface IStaticFieldWrittenFromInstanceMember
    {
        private static int count = 0;
//                         ^^^^^^^^^ Secondary
//                         ^^^^^^^^^ Secondary@-1

        public void DoSomething()
        {
            count++;  // Noncompliant {{Remove this set, which updates a 'static' field from an instance method.}}
//          ^^^^^
            var action = new Action(() =>
            {
                count++; // Compliant, already reported on this symbol
            });
        }

        public static void DoSomethingStatic()
        {
            count++;
        }

        public int MyProperty
        {
            get { return 0; }
            set
            {
                count++; // Noncompliant {{Remove this set, which updates a 'static' field from an instance property.}}
                count += 42; // Compliant, already reported on this symbol
            }
        }
    }

    public partial class PartialClass
    {
        static int field = 1;
//                 ^^^^^^^^^ Secondary
//                 ^^^^^^^^^ Secondary@-1

    }

    public partial class PartialClass
    {
        void Foo()
        {
            field++; // Noncompliant
        }
        void Bar() => field++; // Noncompliant
    }

    public class NotCompileable : NotKnownType // Error [CS0246]
    {
        private static int count = 0; // Secondary
        void Method()
        {
            count++; // Noncompliant
        }
    }
}
