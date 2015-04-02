using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class EmptyCatch
    {
        public void Test()
        {
            try
            {

            }
            catch (Exception exc) //Noncompliant
            {
                /*some comment here*/
            }

            try
            {

            }
            catch (ArgumentException)
            {
                Console.WriteLine("log");
            }

            try
            {

            }
            catch (ArgumentException) //Noncompliant
            {
            }
            catch (Exception) //Noncompliant
            {
            }
        }
    }
}
