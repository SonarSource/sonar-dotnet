using System;

namespace Tests.TestCases
{
    class CatchRethrow
    {
        private void doSomething() { throw new NotSupportedException(); }
        public void Test()
        {
            try
            {
                doSomething();
            }
            catch (Exception exc) //Noncompliant
            {
                throw;
            }

            try
            {
                doSomething();
            }
            catch (ArgumentException) //Noncompliant
            {
                throw;
            }

            try
            {
                doSomething();
            }
            catch (ArgumentException) 
            {
                throw;
            }
            catch (NotSupportedException) //Noncompliant
            {
                throw;
            }

            try
            {
                doSomething();
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
