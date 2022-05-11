using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
            catch // Noncompliant {{Log caught exceptions via ILogger with LogLevel Warning, Error, or Critical}}
//          ^^^^^
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
            catch (Exception ex) // Compliant
            {
                log.LogError(ex, "");
                return new EmptyResult();
            }
        }

        [FunctionName("Function1")]
        public static async Task<IActionResult> LogExceptionInWrappedLogger([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                return new EmptyResult();
            }
            catch (Exception ex) // Compliant
            {
                var nullLogger = NullLogger.Instance;
                nullLogger.Log(LogLevel.Error, new EventId(), (object)null, ex, (s, e) => string.Empty);
                return new EmptyResult();
            }
        }

        private static bool True(Action a)
        {
            a();
            return true;
        }

        [FunctionName("Function1")]
        public static async Task<IActionResult> LogExceptionInExceptionFilter([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                return new EmptyResult();
            }
            catch (Exception ex) when (True(() => log.LogError(ex, ""))) // Compliant
            {
                return new EmptyResult();
            }
        }

        [FunctionName("Function1")]
        public static async Task<IActionResult> LogExceptionInExceptionFilterWrongLogLevel([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                return new EmptyResult();
            }
            catch (Exception ex) when              // Noncompliant
                (True(() => log.LogTrace(ex, ""))) // Secondary
//                          ^^^^^^^^^^^^^^^^^^^^
            {
                return new EmptyResult();
            }
        }

        private static void LoggerHelper(ILogger logger) { }

        [FunctionName("Function1")]
        public static async Task<IActionResult> LoggerGetsPassedToFunction([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                LoggerHelper(log); // Compliant. We don't follow the call, but we assume some decent logging takes place, if the logger gets passed along.
                return new EmptyResult();
            }
        }

        [FunctionName("Function1")]
        public static async Task<IActionResult> WrappedLoggerGetsPassedToFunction([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                LoggerHelper(NullLogger.Instance); // Compliant. Some ILogger is passed
                return new EmptyResult();
            }
        }

        [FunctionName("Function1")]
        public static async Task<IActionResult> NoILoggerInTheEntryPoint([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            try
            {
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                return new EmptyResult(); // Compliant. No (optional) ILogger parameter in the entry point.
            }
        }
    }
}
