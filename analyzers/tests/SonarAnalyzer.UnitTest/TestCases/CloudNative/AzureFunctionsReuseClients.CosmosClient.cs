namespace FunctionApp1
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.WebJobs;

    public static class Function1
    {
        [FunctionName("Sample")]
        public static void Run()
        {
            using (var client = new CosmosClient("connectionString")) // Noncompliant {{Reuse client instances rather than creating new ones with each function invocation.}}
//                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
            }
        }
    }
}
