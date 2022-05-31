namespace FunctionApp1
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class Function1
    {
        const string sampleUrl = @"http://example.com";

        [FunctionName("DefaultSample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var client = new HttpClient();  // Noncompliant {{Reuse client instances rather than creating new ones with each function invocation.}}
//                       ^^^^^^^^^^^^^^^^
            var result = await client.GetAsync(sampleUrl);
            return new OkObjectResult(await result.Content.ReadAsStringAsync());
        }
    }
}
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
        private static HttpClient client = new HttpClient(); // Compliant
        private static readonly Lazy<HttpClient> lazyClient = new Lazy<HttpClient>(() => new HttpClient()); // Compliant
        private static object _lock = new object();
        private static object someField;

        protected static HttpClient ClientProperty { get; set; } = new HttpClient(); // Compliant

        static FunctionApp1()
        {
            ClientProperty = new HttpClient(); // Compliant
        }

        [FunctionName("Sample")]
        public static void Assignments()
        {
            client = new HttpClient(); // FN
            ClientProperty = new HttpClient(); // FN
            var local = new HttpClient(); // Noncompliant
            local = new System.Net.Http.HttpClient(); // Noncompliant
            var otherClient = new UriBuilder(); // Compliant
        }

        [FunctionName("Sample")]
        public static void AssignInCondition()
        {
            if (client == null)
            {
                client = new HttpClient(); // Compliant
            }
        }

        [FunctionName("Sample")]
        public static void AssignInConditionWithLock()
        {
            if (client == null)
            {
                lock (_lock)
                {
                    if (client == null)
                    {
                        client = new HttpClient(); // Compliant
                    }
                }
            }
        }

        [FunctionName("Sample")]
        public static void AssignWithClientFactory(IHttpClientFactory factory)
        {
            var local = factory.CreateClient("SomeName"); // Compliant
        }

        [FunctionName("Sample")]
        public static void AssigntoLocal()
        {
            var local = new HttpClient(); // Noncompliant
        }

        [FunctionName("Sample")]
        public static void WrapInUsingBlock()
        {
            using (var local = new HttpClient()) // Noncompliant
            {
            }
        }

        [FunctionName("Sample")]
        public static void NoAssignment()
        {
            new HttpClient(); // Noncompliant
        }

        [FunctionName("Sample")]
        public static async Task NoAssignmentAndCall()
        {
            await new HttpClient().GetStringAsync(@"http://example.com"); // Noncompliant
        }

        [FunctionName("Sample")]
        public static async Task AssignmentOfInvocationResult()
        {
            someField = await new HttpClient().GetStringAsync(@"http://example.com"); // Noncompliant
        }
    }
}
