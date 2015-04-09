using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class IdenticalExpressionsInBinaryOp
    {
        public void doZ() { throw new Exception();}
        public void doW() { throw new Exception();}
        public void Test(bool a, bool b)
        {
            if (a == a) //Noncompliant
            {
                doZ();
            }

            if (a == b || a == /*comment*/ b) //Noncompliant
            {
                doW();
            }

            int j = 5 / 5; //Noncompliant
            int k = 5 - 5; //Noncompliant
            int l = 5 * 5;

            int i = 1 << 1;
            i = 2 << 2; //Noncompliant
        }
    }
}
