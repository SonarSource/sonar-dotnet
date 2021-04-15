using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalStructureSameCondition1
    {
        public bool condition { get; set; }
        public bool condition1 { get; set; }
        public bool condition2 { get; set; }

        public void SimpleTest()
        {
            var b = true;
            if (b && condition)
//              ^^^^^^^^^^^^^^ Secondary
            {

            }
            else if (b && condition) // Noncompliant {{This branch duplicates the one on line 18.}}
//                   ^^^^^^^^^^^^^^
            {

            }
        }

        public void Test()
        {
            var b = true;
            if (b && condition)
//              ^^^^^^^^^^^^^^ Secondary
//              ^^^^^^^^^^^^^^ Secondary@-1
//              ^^^^^^^^^^^^^^ Secondary@-2
            {

            }
            else if (b && condition) // Noncompliant {{This branch duplicates the one on line 33.}}
//                   ^^^^^^^^^^^^^^
            {

            }
            else if (b && condition) // Noncompliant
            {

            }
            else if (!b && condition)
//                   ^^^^^^^^^^^^^^^ Secondary
            {

            }
            else if (b && /*some comment*/ condition) // Noncompliant
            {

            }
            else if (!b && condition) // Noncompliant
            {

            }

            if (condition1)
//              ^^^^^^^^^^ Secondary
            {
            }
            else if (condition1) // Noncompliant
            {
            }

            if (condition2)
            {
            }
            else if (condition1)
//                   ^^^^^^^^^^ Secondary
            {
            }
            else if (condition1) // Noncompliant
            {
            }

            if (condition1)
//              ^^^^^^^^^^ Secondary
            {
            }
            else if (condition2)
            {
            }
            else if (condition1) // Noncompliant
            {
            }
        }
    }
}
