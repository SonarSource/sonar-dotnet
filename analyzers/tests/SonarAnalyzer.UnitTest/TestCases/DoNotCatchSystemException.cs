using System;
using System.IO;

namespace Tests.Diagnostics
{
    public class DummyType : Exception { }

    class Program
    {
        public void Method()
        {
            try
            {
                // do something that might throw a FileNotFoundException or IOException
            }
            catch (Exception e) // Noncompliant {{Catch a list of specific exception subtype or use exception filters instead.}}
//                 ^^^^^^^^^
            {
                // log exception ...
            }

            try
            {
                // do something
            }
            catch (Exception e) when (e is FileNotFoundException || e is IOException)
            {
                // do something
            }

            try
            {
                // do something
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException || e is IOException)
                {
                    // do something
                }
                else
                {
                    throw;
                }
            }

            try { }
            catch (Exception e)
            {
                throw;
            }

            try { }
            catch (Exception e)
            {
                throw e;
            }

            try { }
            catch (Exception) // Noncompliant
            {
                new Exception();
            }

            try { }
            catch (IOException)
            {
                new Exception();
            }

            try { }
            catch (Exception e)
            {
                throw new Exception();
            }

            try { }
            catch // Noncompliant
//          ^^^^^
            {
            }

            try { }
            catch (DummyType)
            {
                throw;
            }
        }
    }
}

namespace AzureFunction
{
    using Microsoft.Azure.WebJobs;

    class Program
    {
        [FunctionName("Sample")]
        public void Method()
        {
            try { }
            catch (Exception e) { } // Compliant. Don't raise for AzureFunctions because it contradicts S6421.

            try { }
            catch (Exception) { }   // Compliant.

            try { }
            catch { }               // Compliant.
        }
    }
}
