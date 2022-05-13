using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;

public static class LogInCatchClause
{
    [FunctionName("Sample")]
    public static void EmptyCatchClause(ILogger log)
    {
        try { }
        catch { } // Noncompliant {{Log exception via ILogger with LogLevel Information, Warning, Error, or Critical.}}
//      ^^^^^
    }

    [FunctionName("Sample")]
    public static void LogLevelInvalid(ILogger log)
    {
        try { }
        catch // Noncompliant
//      ^^^^^
        {
            log.LogTrace(string.Empty);        // Secondary
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^
            log.LogDebug(string.Empty);        // Secondary
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionInCatchClause(ILogger log)
    {
        try { }
        catch (Exception ex) // Compliant
        {
            log.LogError(ex, "");
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionInWrappedLogger(ILogger log)
    {
        try { }
        catch (Exception ex) // Compliant
        {
            var nullLogger = NullLogger.Instance;
            nullLogger.Log(LogLevel.Error, new EventId(), (object)null, ex, (s, e) => string.Empty);
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionLoggedInLocalFunction(ILogger log)
    {
        try { }
        catch (Exception ex) // FN. Any call to "log" is considered valid. Reachability is not considered.
        {
            void Log() => log.LogError("");
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionLoggedInLambda(ILogger log)
    {
        try { }
        catch (Exception ex) // FN. Any call to "log" is considered valid. Reachability is not considered.
        {
            Action x = () => log.LogError("");
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionLoggedInAnonymousFunction(ILogger log)
    {
        try { }
        catch (Exception ex) // FN. Any call to "log" is considered valid. Reachability is not considered.
        {
            Action x = delegate () { log.LogError(""); };
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionIsPassedInLambda(ILogger log)
    {
        try { }
        catch (Exception ex) // FN. Any call to "log" is considered valid. Reachability is not considered.
        {
            DoSomething(() => log.LogError(""));
        }

        void DoSomething(Action a) => a();
    }

    [FunctionName("Sample")]
    public static void LogExceptionLoggedInUnreachableCode(ILogger log)
    {
        try { }
        catch (Exception ex) // FN. log in unreachable code.
        {
            return;
            log.LogError("");
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionNotEveryCodePathContainsLog(ILogger log)
    {
        try { }
        catch (Exception ex) // FN. Not every code path contains a log
        {
            var i = 100;
            if (i == 42)
            {
                log.LogError("");
            }
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionFromWebApi(ILogger log)
    {
        try { }
        catch (Exception ex) // Compliant. Call to unregcognized extension method from WebApi
        {
            var i = 100;
            if (i == 42)
            {
                log.LogMetric("Metric", 5);
            }
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionInNestedCatch(ILogger log)
    {
        try { }
        catch (Exception ex) // FN. The outer block should contain a log call too
        {
            try { }
            catch
            {
                log.LogError("");
            }
        }
    }

    [FunctionName("Sample")]
    public static void LoggerGetsPassedToFunction(ILogger log)
    {
        try { }
        catch (Exception ex)
        {
            LoggerHelper(log); // Compliant. We don't follow the call, but we assume some decent logging takes place, if the logger gets passed along.
        }
    }

    [FunctionName("Sample")]
    public static void WrappedLoggerGetsPassedToFunction(ILogger log)
    {
        try { }
        catch (Exception ex)
        {
            LoggerHelper(NullLogger.Instance); // Compliant. Some ILogger is passed
        }
    }

    [FunctionName("Sample")]
    public static void CustomExtensionMethod(ILogger log)
    {
        try { }
        catch (Exception ex)
        {
            log.LogInformationCustomExtension(""); // Compliant. Custom extension methods are considered compliant.
        }
    }

    [FunctionName("Sample")]
    public static void NoILoggerInTheEntryPoint([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
    {
        try { }
        catch (Exception ex) // Compliant. No (optional) ILogger parameter in the entry point.
        {
        }
    }

    private static void LoggerHelper(ILogger logger) { }
}

// https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
public class DependencyInjectionField
{
    private readonly ILogger logger;

    // Scenario: ILogger service was registered in FunctionsStartup and inject via constructor injection:
    // In FunctionsStartup: public override void Configure(IFunctionsHostBuilder builder) => builder.Services.Add<ILogger>(...);
    // Here: public DependencyInjectionField(ILogger logger) => this.logger = logger;

    [FunctionName("Sample")]
    public void InjectedLoggerIsUsed()
    {
        try { }
        catch
        {
            logger.LogError(""); // Compliant
        }
    }

    [FunctionName("Sample")]
    public void InjectedLoggerFieldIsNotUsed()
    {
        try { }
        catch { } // Noncompliant
    }
}

public class DependencyInjectionProperty
{
    private ILogger Logger { get; }

    [FunctionName("Sample")]
    public void InjectedLoggerPropertyIsNotUsed()
    {
        try { }
        catch { } // Noncompliant
    }
}

public class DependencyInjectionMethod
{
    protected ILogger Logger() => null;

    [FunctionName("Sample")]
    public void InjectedLoggerMethodIsNotUsed()
    {
        try { }
        catch { } // Noncompliant
    }
}

public class Derived: DependencyInjectionMethod
{
    [FunctionName("Sample")]
    public void InjectedLoggerFromBaseIsNotUsed()
    {
        try { }
        catch { } // Noncompliant
    }
}

public class GenericLogger
{
    private ILogger<GenericLogger> logger;

    [FunctionName("Sample")]
    public void InjectedLoggerFromBaseIsNotUsed()
    {
        try { }
        catch { } // Noncompliant
    }
}

// See https://blog.stephencleary.com/2020/06/a-new-pattern-for-exception-logging.html
public static class LogInExceptionFilter
{
    [FunctionName("Sample")]
    public static void LogExceptionInExceptionFilter(ILogger log)
    {
        try { }
        catch (Exception ex) when (True(() => log.LogError(ex, ""))) // Compliant
        {
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionInExceptionFilterWrongLogLevel(ILogger log)
    {
        try { }
        catch (Exception ex) when              // Noncompliant
            (True(() => log.LogTrace(ex, ""))) // Secondary
//                      ^^^^^^^^^^^^^^^^^^^^
        {
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionInExceptionFilterCustomExtensionMethod(ILogger log)
    {
        try { }
        catch (Exception ex) when              // Compliant
            (log.LogInformationCustomExtension(""))
        {
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionInExceptionFilterLocalLambdaCapture(ILogger log)
    {
        Func<bool> exceptionFilter = () => { log.LogError(""); return true; };
        try { }
        catch (Exception ex) when (exceptionFilter()) // Noncompliant. FP
        {
        }
    }

    [FunctionName("Sample")]
    public static void LogExceptionInExceptionFilterLocalFunctionOutsideCatchCapturesILogger(ILogger log)
    {
        try { }
        catch (Exception ex) when (ExceptionFilter()) // Noncompliant. FP
        {
        }

        bool ExceptionFilter() { log.LogError(""); return true; }
    }

    private static bool True(Action a) => true; // Takes the logging call executes it and returns true for the exception filter.
}

public static class CustomLoggerExtensions
{
    public static bool LogInformationCustomExtension(this ILogger logger, string message) => true;
}
