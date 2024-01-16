namespace FunctionApp1
{
    using Azure.Identity;
    using Azure.ResourceManager;
    using Microsoft.Azure.WebJobs;

    public static class Function1
    {
        [FunctionName("Sample")]
        public static void Run()
        {
            var armClient = new ArmClient(new DefaultAzureCredential()); // Noncompliant {{Reuse client instances rather than creating new ones with each function invocation.}}
        }
    }
}
