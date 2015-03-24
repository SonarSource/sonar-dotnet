using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalStructureSameCondition
    {
        public bool condition { get; set; }
        public void Test()
        {
            var b = true;
            if (b && condition)
            {

            }
            else if (b && this.condition) // Noncompliant
            {

            }
            else if (b && condition) // Noncompliant
            {

            }
            else if (!b && this.condition)
            {

            }
            else if (b && /*some comment*/ condition) // Noncompliant
            {

            }
            else if (!b && condition) // Noncompliant
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
