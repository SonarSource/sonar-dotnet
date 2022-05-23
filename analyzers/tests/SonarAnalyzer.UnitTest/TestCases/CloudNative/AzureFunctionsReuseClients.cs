using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp1
{
    public static class Function1
    {
        const string sampleUrl = @"http://example.com";

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var client = new HttpClient();
            var result = await client.GetAsync(sampleUrl);
            return new OkObjectResult(await result.Content.ReadAsStringAsync());
        }
    }
}
