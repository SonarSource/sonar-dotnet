using System;

namespace Tests.Diagnostics
{
    class Program
    {
        void Foo()
        {
            if (true)
            {
            } if (true)
//            ^^ Noncompliant {{Move this 'if' to a new line or add the missing 'else'.}}
//          ^ Secondary@-1
            {
            }


            if (true)
            {
                // ...
            }
            else if (true)
            {
                //...
            }

            if (true)
            {
                // ...
            }

            if (true)
            {
                //...
            }

            Action a = () => { };
            {} a(); if (true) { }

            /*}*/  if (true)
            {
            } /* else */ if (true) { } // Noncompliant
// Secondary@-1

            else { } { if (true)
                {

                }
            }


            if (true) {
            } else if (true) {
            }

            if (true)
            {
            }
            if (true)
            {
            }
        }
    }
}
