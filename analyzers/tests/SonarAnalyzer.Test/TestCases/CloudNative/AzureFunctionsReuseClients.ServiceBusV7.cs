namespace FunctionApp1
{
    using Azure.Messaging.ServiceBus;
    using Azure.Messaging.ServiceBus.Administration;
    using Microsoft.Azure.WebJobs;

    public static class Function1
    {
        [FunctionName("Sample")]
        public static void Run()
        {
            var serviceBus = new ServiceBusClient("connectionString");          // Noncompliant {{Reuse client instances rather than creating new ones with each function invocation.}}
            var admin = new ServiceBusAdministrationClient("connectionString"); // Noncompliant
        }
    }
}
