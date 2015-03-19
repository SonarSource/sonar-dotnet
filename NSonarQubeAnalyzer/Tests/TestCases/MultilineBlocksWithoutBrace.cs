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
            while (true) 
                Tests();
                Tests(); // Noncompliant

            while (true) 
Tests();
Tests(); // Noncompliant

            if (true)
                Tests();
            Tests();

            while (true)
            {
                Tests();
            }
            Tests();

            if (true) 
                Tests();

                Tests(); // Noncompliant

            if (true) 
                Tests();
            else 
                Tests();
                Tests(); // Noncompliant

            while (true) 
                Tests();
   /*comment*/  Tests(); // Noncompliant

            while (true) 
                Tests();
            /*comment*/
                Tests(); // Noncompliant
        }
    }
}
