namespace FunctionApp1
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;

    public static class Function1
    {
        const string sampleUrl = @"http://example.com";

        [FunctionName("DefaultSample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var queue = new QueueClient("connectionString", "entityPath");                           // Noncompliant {{Reuse client instances rather than creating new ones with each function invocation.}}
            var session = new SessionClient("connectionString", "entityPath");                       // Noncompliant
            var topic = new TopicClient("connectionString", "entityPath");                           // Noncompliant
            var subscription = new SubscriptionClient("connectionString", "topic", "subscription");  // Noncompliant
            var management = new ManagementClient("connectionString");                               // Noncompliant
            return new UnauthorizedResult();
        }
    }
}
