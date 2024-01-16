namespace FunctionApp1
{
    using System;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.WebJobs;

    public static class Function1
    {
        [FunctionName("Sample")]
        public static void Run()
        {
            using (var client = new DocumentClient(new Uri("https://example.com"), "token")) // Noncompliant {{Reuse client instances rather than creating new ones with each function invocation.}}
//                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
            }
        }
    }
}
