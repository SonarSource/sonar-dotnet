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

            int l = 5 | 5; // Noncompliant
            // Secondary@-1
            l |= (l); // Noncompliant
            // Secondary@-1

            int i = 1;

            object.Equals(i, i);
//                        ^ {{Change one instance of 'i' to a different value; comparing 'i' to itself always returns true.}}
//                           ^ Secondary@-1

            var o = new object();
            o.Equals(o);
//          ^ {{Change one instance of 'o' to a different value; comparing 'o' to itself always returns true.}}
//                   ^ Secondary@-1

            (new object()).Equals(new object());
//          ^^^^^^^^^^^^^^ {{Change one instance of 'new object()' to a different value; comparing 'new object()' to itself always returns true.}}
//                                ^^^^^^^^^^^^ Secondary@-1

            Foo f = new Foo();
            f.Equals(f); // Noncompliant
            // Secondary@-1

        }

        public void CompliantCases()
        {
            int i = 1 << 1;
            i = 1 << 0x1;
            i = 2 << 2;
            i = 2 + 2;
            i = 2 * 2;

            i = i;
        }
    }
}
