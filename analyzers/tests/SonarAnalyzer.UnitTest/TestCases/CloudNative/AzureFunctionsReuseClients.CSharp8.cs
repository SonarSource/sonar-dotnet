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
        private static HttpClient client;

        [FunctionName("Sample")]
        public static void NullConditionalAssignment()
        {
            client ??= new HttpClient(); // Compliant
        }

        [FunctionName("Sample")]
        public static void WrapInUsingDeclaration()
        {
            using var local = new HttpClient(); // Noncompliant
        }
    }
}
