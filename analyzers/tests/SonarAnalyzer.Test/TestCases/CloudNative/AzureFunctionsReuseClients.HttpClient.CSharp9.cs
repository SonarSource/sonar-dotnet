namespace DifferentAssignments
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class FunctionApp1
    {
        private static HttpClient field;

        [FunctionName("Sample")]
        public static void DifferentAssigments()
        {
            field ??= new HttpClient();                       // Compliant

            var local = default(HttpClient);
            local ??= new HttpClient();                       // FN, needs SE to be able to raise an issue.

            _ = new HttpClient();                             // Noncompliant
            using var localUsingStatement = new HttpClient(); // Noncompliant
            HttpClient targetTypedNew = new();                // Noncompliant
            field = new();                                    // Compliant

            PassThrough(field = new());                       // Compliant
        }

        private static HttpClient PassThrough(HttpClient httpClient) => httpClient;
    }
}
