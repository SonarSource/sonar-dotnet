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
                throw; // Fixed
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
                    throw; // Fixed
                }
                catch (Exception exc)
                {
                    throw; // Fixed
                    throw exception;
                }
            }
        }
    }
}
