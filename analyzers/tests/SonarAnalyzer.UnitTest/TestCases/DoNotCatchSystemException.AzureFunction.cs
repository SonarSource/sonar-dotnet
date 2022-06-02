using System;
using Microsoft.Azure.WebJobs;

namespace Tests.Diagnostics
{
    class Program
    {
        [FunctionName("Sample")]
        public void Method()
        {
            try
            {
            }
            catch (Exception e)    // Compliant. Don't raise for AzureFunctions because it contradicts S6421.
            {
            }

            try { }
            catch (Exception) { } // Compliant.

            try { }
            catch { }             // Compliant.
        }
    }
}
