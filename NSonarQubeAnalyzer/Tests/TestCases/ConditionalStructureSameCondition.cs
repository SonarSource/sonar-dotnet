using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalStructureSameCondition
    {
        public void Test()
        {
            var b = true;
            if (b && b)
            {

            }
            else if (b &&  b) // Noncompliant
            {

            }
            else if (b && b    ) // Noncompliant
            {

            }
            else if (!b && b)
            {

            }
            else if (b && /*some comment*/ b) // Noncompliant
            {

            }
            else if (!b && b) // Noncompliant
            {

            }

            if (condition1)
            {
            }
            else if (condition1) // Noncompliant
            { 
            }

            if (condition2)
            {
            }
            else if (condition1)
            {
            }
            else if (condition1) // Noncompliant
            { 
            }

            if (condition1)
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
