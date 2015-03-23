using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class CatchRethrow
    {
        public void Test()
        {
            try
            {

            }
            catch (Exception exc) //Noncompliant
            {
                throw;
            }

            try
            {

            }
            catch (ArgumentException) //Noncompliant
            {
                throw;
            }

            try
            {

            }
            catch (ArgumentException) 
            {
                throw;
            }
            catch (Exception) //Noncompliant
            {
                throw;
            }

            try
            {

            }
            catch (ArgumentException)
            {
                Console.WriteLine("");
                throw;
            }
            catch (Exception)
            {
                Console.WriteLine("");
                throw;
            }
        }
    }
}
