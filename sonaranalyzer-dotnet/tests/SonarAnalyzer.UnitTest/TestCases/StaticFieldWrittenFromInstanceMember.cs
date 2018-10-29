using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;

namespace Tests.Diagnostics
{
    class StaticFieldWrittenFromInstanceMember
    {
        private static int count = 0;
//                         ^^^^^^^^^ Secondary [0]
//                         ^^^^^^^^^ Secondary@-1 [1]
        private int countInstance = 0;

        public void DoSomething()
        {
            count++;  // Noncompliant [0] {{Make the enclosing instance method 'static' or remove this set on the 'static' field.}}
//          ^^^^^
            var action = new Action(() =>
            {
                count++; // Compliant, already reported on this symbol
            });
            countInstance++;
        }

        public static void DoSomethingStatic()
        {
            count++;
        }


        public int MyProperty
        {
            get { return myVar; }
            set
            {
                count++; // Noncompliant [1]
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
}
