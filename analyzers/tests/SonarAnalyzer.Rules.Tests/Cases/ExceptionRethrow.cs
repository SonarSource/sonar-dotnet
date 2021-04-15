using System;

namespace Tests.Diagnostics
{
    public class ExceptionType1 : Exception { }
    public class ExceptionType2 : Exception { }

    public class ExceptionRethrow
    {
        public void Test()
        {
            try
            { }
            catch (ExceptionType1 exc)
            {
                Console.WriteLine(exc);
                throw exc; // Noncompliant {{Consider using 'throw;' to preserve the stack trace.}}
//              ^^^^^^^^^^
                throw;
            }
            catch (ExceptionType2 exc)
            {
                throw new Exception("My custom message", exc);  // Compliant; stacktrace preserved
            }

            try
            { }
            catch (Exception)
            {
                throw;
            }

            try
            {
            }
            catch (Exception exception)
            {
                try
                {
                    throw exception; // Noncompliant
                }
                catch (Exception exc)
                {
                    throw exc; // Noncompliant
                    throw exception;
                }
            }
        }
    }
}
