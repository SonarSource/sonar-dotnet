using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class Foo { }

    class BinaryOperationWithIdenticalExpressions
    {
        public void doZ() { throw new Exception();}
        public void doW() { throw new Exception();}
        public void Test(bool a, bool b)
        {
            if (a == a)
//                   ^ {{Correct one of the identical expressions on both sides of operator '=='.}}
//              ^ Secondary@-1
            {
                doZ();
            }

            if (a == b || (a == /*comment*/ b))
//                        ^^^^^^^^^^^^^^^^^^^^ {{Correct one of the identical expressions on both sides of operator '||'.}}
//              ^^^^^^ Secondary@-1
            {
                doW();
            }

            int j = 5 / 5; //Noncompliant
            // Secondary@-1
            int k = 5 - 5; //Noncompliant
            // Secondary@-1
            int l = 5 * 5;

            l = 5 | 5; // Noncompliant
            // Secondary@-1
            l |= (l); // Noncompliant
            // Secondary@-1

            int i = 1 << 1;
            i = 1 << 0x1;
            i = 2 << 2; // Compliant

            object.Equals(i, i);
//                        ^ {{Correct one of the identical expressions on both sides of Object.Equals.}}
//                           ^ Secondary@-1

            var o = new object();

            o.Equals(o);
//          ^ {{Correct one of the identical expressions on both sides of Object.Equals.}}
//                   ^ Secondary@-1

            (new object()).Equals(new object());
//          ^^^^^^^^^^^^^^ {{Correct one of the identical expressions on both sides of Object.Equals.}}
//                                ^^^^^^^^^^^^ Secondary@-1

            Foo f;
            f.Equals(f); // Noncompliant
            // Secondary@-1

        }
    }
}
