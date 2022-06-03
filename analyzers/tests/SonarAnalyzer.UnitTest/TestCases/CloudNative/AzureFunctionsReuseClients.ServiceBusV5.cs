namespace FunctionApp1
{
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;
    using Microsoft.Azure.WebJobs;

    public static class Function1
    {
        [FunctionName("Sample")]
        public static void Run()
        {
            var queue = new QueueClient("connectionString", "entityPath");                           // Noncompliant {{Reuse client instances rather than creating new ones with each function invocation.}}
            var session = new SessionClient("connectionString", "entityPath");                       // Noncompliant
            var topic = new TopicClient("connectionString", "entityPath");                           // Noncompliant
            var subscription = new SubscriptionClient("connectionString", "topic", "subscription");  // Noncompliant
            var management = new ManagementClient("connectionString");                               // Noncompliant
        }
    }
}
