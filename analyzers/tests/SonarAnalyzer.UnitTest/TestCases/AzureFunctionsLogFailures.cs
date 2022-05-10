using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureFunctions1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> EmptyCatchClause([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                return new EmptyResult();
            }
            catch // Noncompliant {{Log caught exceptions via ILogger}}
        //  ^^^^^
            {
                return new EmptyResult();
            }
        }

        [FunctionName("Function1")]
        public static async Task<IActionResult> LogExceptionInCatchClause([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                return new EmptyResult();
            }
            catch(Exception ex) // Compliant
            {
                log.LogError(ex, "");
                return new EmptyResult();
            }
        }
    }
}
