using System;

namespace Tests.Diagnostics
{
    public class FunctionComplexity
    {
        private bool field;

        public FunctionComplexity()
//             ^^^^^^^^^^^^^^^^^^ Noncompliant [0] {{The Cyclomatic Complexity of this constructor is 4 which is greater than 3 authorized.}}
//             ^^^^^^^^^^^^^^^^^^ Secondary@-1 [0] {{+1}}
        {
            if (false) { }
//          ^^ Secondary [0] {{+1}}
            if (false) { }
//          ^^ Secondary [0] {{+1}}
            if (false) { }
//          ^^ Secondary [0] {{+1}}
        }

        ~FunctionComplexity()
//       ^^^^^^^^^^^^^^^^^^ Noncompliant [1]
//       ^^^^^^^^^^^^^^^^^^ Secondary@-1 [1] {{+1}}
        {
            if (false) { }
//          ^^ Secondary [1] {{+1}}
            if (false) { }
//          ^^ Secondary [1] {{+1}}
            if (false) { }
//          ^^ Secondary [1] {{+1}}
        }

        public void M1()
        {
            if (false) { }
            if (false) { }
        }

        public void M2() // Noncompliant [2]
                         // Secondary@-1 [2] {{+1}}
        {
            if (false) { } // Secondary [2] {{+1}}
            if (false) { } // Secondary [2] {{+1}}
            if (false) { } // Secondary [2] {{+1}}
        }

        public int MyProperty
        {
            get // Noncompliant [3]
                // Secondary@-1 [3] {{+1}}
            {
                if (false) { } // Secondary [3] {{+1}}
                if (false) { } // Secondary [3] {{+1}}
                if (false) { } // Secondary [3] {{+1}}
                return 0;
            }
            set // Noncompliant [4]
                // Secondary@-1 [4] {{+1}}
            {
                if (false) { } // Secondary [4] {{+1}}
                if (false) { } // Secondary [4] {{+1}}
                if (false) { } // Secondary [4] {{+1}}
            }
        }

        public event EventHandler OnSomething
        {
            add // Noncompliant [5]
                // Secondary@-1 [5] {{+1}}
            {
                if (false) { } // Secondary [5] {{+1}}
                if (false) { } // Secondary [5] {{+1}}
                if (false) { } // Secondary [5] {{+1}}
            }
            remove // Noncompliant [6]
                   // Secondary@-1 [6] {{+1}}
            {
                if (false) { } // Secondary [6] {{+1}}
                if (false) { } // Secondary [6] {{+1}}
                if (false) { } // Secondary [6] {{+1}}
            }
        }

        public static FunctionComplexity operator +(FunctionComplexity a) // Noncompliant [7]
        // Secondary@-1 [7] {{+1}}
        {
            if (false) { } // Secondary [7] {{+1}}
            if (false) { } // Secondary [7] {{+1}}
            if (false) { } // Secondary [7] {{+1}}
            return null;
        }

        public bool Method23(bool x) => x || x || x || x || x; // Noncompliant [8]
        // Secondary@-1 [8] {{+1}}
        // Secondary@-2 [8] {{+1}}
        // Secondary@-3 [8] {{+1}}
        // Secondary@-4 [8] {{+1}}
        // Secondary@-5 [8] {{+1}}

        public bool Prop => field || field || field || field || field; // Noncompliant [9]
        // Secondary@-1 [9] {{+1}}
        // Secondary@-2 [9] {{+1}}
        // Secondary@-3 [9] {{+1}}
        // Secondary@-4 [9] {{+1}}

        private void Foo(Class c) // Noncompliant [10]
                                  // Secondary@-1 [10] {{+1}}
        {
            var x = c?.Name?.ToString();
//                   ^ Secondary [10] {{+1}}
//                         ^ Secondary@-1 [10] {{+1}}

            if (false) { } // Secondary [10] {{+1}}
        }

        private class Class
        {
            public string Name;
        }
    }
}
