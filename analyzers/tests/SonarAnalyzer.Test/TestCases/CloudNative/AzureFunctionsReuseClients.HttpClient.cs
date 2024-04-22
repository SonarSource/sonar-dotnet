using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("DefaultSample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var client = new HttpClient();  // Noncompliant {{Reuse client instances rather than creating new ones with each function invocation.}}
//                       ^^^^^^^^^^^^^^^^
            var result = await client.GetAsync("");
            return new OkObjectResult("");
        }
    }
}

namespace DifferentAssignments
{
    public class FunctionApp1
    {
        private static HttpClient client = new HttpClient(); // Compliant
        private static readonly Lazy<HttpClient> lazyClient = new Lazy<HttpClient>(() => new HttpClient()); // Compliant
        private static HttpClient parenthesesInInitialzer = (new HttpClient()); // Compliant
        private static object castInInitialzer = (object)(new HttpClient());    // Compliant
        private static object _lock = new object();
        private static object someField;

        protected static HttpClient ClientProperty { get; set; } = new HttpClient(); // Compliant
        protected static Lazy<HttpClient> LazyClientProperty { get; set; } = new Lazy<HttpClient>(() => new HttpClient()); // Compliant

        protected HttpClient ClientPropertyAccessor { get => new HttpClient(); } // FN
        protected HttpClient ClientPropertyAccessorArrow => new HttpClient();    // FN

        static FunctionApp1()
        {
            ClientProperty = new HttpClient(); // Compliant
        }

        [FunctionName("Sample")]
        public static void Assignments()
        {
            client = new HttpClient();                           // Compliant. The field is unconditionally assigned on each call, but we don't do SymbolicExecution analysis
            FunctionApp1.client = new HttpClient();              // Compliant
            ClientProperty = new HttpClient();                   // Compliant
            FunctionApp1.ClientProperty = new HttpClient();      // Compliant
            ClientProperty = (new HttpClient());                 // Compliant
            someField = (object)(new HttpClient());              // Noncompliant FP. Some trickery to confuse the analyzer.
            someField = (new HttpClient() as object);            // Noncompliant FP
            client = PassThrough(new HttpClient());              // Noncompliant FP
            client = client ?? new HttpClient();                 // Noncompliant FP
            client = client == null ? new HttpClient() : client; // Compliant
            PassThrough(new HttpClient());                       // Noncompliant
            var local = new HttpClient();                        // Noncompliant
            local = new System.Net.Http.HttpClient();            // Noncompliant
            PassThrough(local = new HttpClient());               // Noncompliant
            PassThrough(client = new HttpClient());              // Compliant
            var otherClient = new UriBuilder();                  // Compliant
        }

        public static void NotAnAzureFunction()
        {
            var local = new HttpClient(); // Compliant
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

        private static HttpClient PassThrough(HttpClient httpClient) => httpClient;
    }
}

namespace DependencyInjection
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
        private HttpClient clientField = new HttpClient();                     // Compliant
        private HttpClient ClientProperty { get; set; } = new HttpClient();    // Compliant
        private HttpClient ClientPropertyAccessor { get => new HttpClient(); } // FN
        private HttpClient ClientPropertyAccessorArrow => new HttpClient();    // FN

        public FunctionApp1(HttpClient httpClient)
        {
            clientField = httpClient;    // Compliant
            ClientProperty = httpClient; // Compliant
        }

        public FunctionApp1()
        {
            clientField = new HttpClient();    // FN. HttpClient should be injected. This is more related to DI than AzureFunctions and should therefore not be detected here.
            ClientProperty = new HttpClient(); // FN
        }

        [FunctionName("Sample")]
        public void Assignments()
        {
            clientField = new HttpClient();           // Noncompliant
            ClientProperty = new HttpClient();        // Noncompliant
            var local = new HttpClient();             // Noncompliant
            local = new System.Net.Http.HttpClient(); // Noncompliant
            local = ClientPropertyAccessor;           // FN
            var otherClient = new UriBuilder();       // Compliant
        }
    }
}
