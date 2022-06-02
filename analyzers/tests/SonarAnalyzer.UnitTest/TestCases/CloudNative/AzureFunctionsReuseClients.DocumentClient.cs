namespace FunctionApp1
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents.Client;

    public static class Function1
    {
        const string sampleUrl = @"http://example.com";

        [FunctionName("DefaultSample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            using (var client = new DocumentClient(new Uri("https://example.com"), "token")) // Noncompliant {{Reuse client instances rather than creating new ones with each function invocation.}}
//                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
                return new UnauthorizedResult();
            }
        }
    }
}
