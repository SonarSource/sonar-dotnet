using System;

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
            catch (AnyException) //Noncompliant
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
