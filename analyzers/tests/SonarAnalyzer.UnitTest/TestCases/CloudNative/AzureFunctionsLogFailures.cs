namespace AzureFunctionLogFailuresTests
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using ILoggerAlias = Microsoft.Extensions.Logging.ILogger;

    public static class LogInCatchClause
    {
        [FunctionName("Sample")]
        public static void EmptyCatchClause(ILogger log)
        {
            try { }
            catch { } // Noncompliant {{Log exception via ILogger with LogLevel Information, Warning, Error, or Critical.}}
//          ^^^^^
        }

        [FunctionName("Sample")]
        public static void LogLevelInvalid(ILogger log)
        {
            try { }
            catch // Noncompliant
//          ^^^^^
            {
                log.LogTrace(string.Empty);        // Secondary
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^
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
        public static void LogExceptionInCatchClauseWithErrorAndTrace(ILogger log)
        {
            try { }
            catch (Exception ex) // Compliant
            {
                log.LogTrace(ex, "");
                log.LogError(ex, "");
                log.LogTrace(ex, "");
            }
        }

        public static void MissingFunctionNameAttribute(ILogger log)
        {
            try { }
            catch { } // Compliant. Not an AzureFunction
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
                DoNothing(() => log.LogError(""));
            }

            void DoNothing(Action a) { }
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
            catch (Exception ex) // Compliant. Microsoft.Azure.WebJobs adds an extension method "LogMetric". It is in scope when the standard AzureFunction is scaffolded in VS, but we only take extension methods comming from the logging package into account.
            {
                log.LogMetric("Metric", 5);
            }
        }

        [FunctionName("Sample")]
        public static void LogExceptionToUndefinedMethod(ILogger log)
        {
            try { }
            catch (Exception ex) // Noncompliant.
            {
                log.Undefined(); // Error [CS1061] ILogger' does not contain a definition for 'Undefined'
            }
        }

        [FunctionName("Sample")]
        public static void CallLogIsEnabled(ILogger log)
        {
            try { }
            catch (Exception ex) // Noncompliant.
            {
                log.IsEnabled(LogLevel.Error); // Secondary
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

        [FunctionName("Sample")]
        public static void LogWithLogLevelTrace(ILogger log)
        {
            try { }
            catch (Exception ex) // Noncompliant
            {
                log.Log(LogLevel.Trace, string.Empty); // Secondary
            }
        }

        [FunctionName("Sample")]
        public static void LogWithLogLevelError(ILogger log)
        {
            try { }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, string.Empty); // Compliant
            }
        }

        [FunctionName("Sample")]
        public static void LogWithNonConstantLogLevel(ILogger log)
        {
            try { }
            catch (Exception ex)
            {
                var logLevel = ex.InnerException == null ? LogLevel.Debug : LogLevel.Trace;
                log.Log(logLevel, string.Empty); // Compliant. Non-constant LogLevels are considered valid.
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
        protected ILogger Logger { get; }

        [FunctionName("Sample")]
        public void InjectedLoggerPropertyIsNotUsed()
        {
            try { }
            catch { } // Noncompliant
        }
    }

    public class DependencyInjectionMethod
    {
        private ILogger Logger() => null;

        [FunctionName("Sample")]
        public void InjectedLoggerMethodIsNotUsed()
        {
            try { }
            catch { } // Compliant. An ILogger retreivable via a method call is not a supported scenario.
        }
    }

    public class DerivedViaProperty : DependencyInjectionProperty
    {
        [FunctionName("Sample")]
        public void InjectedLoggerFromBaseIsNotUsed()
        {
            try { }
            catch { } // Noncompliant. ILogger is accessible via the property in the base class.
        }
    }

    public class DerivedViaField : DependencyInjectionField
    {
        [FunctionName("Sample")]
        public void InjectedLoggerFromBaseIsNotAccessible()
        {
            try { }
            catch { } // Compliant. ILogger is a private field in the base class and not accessible.
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

    public class UseILoggerAlias
    {
        private ILoggerAlias logger;

        [FunctionName("Sample")]
        public void InjectedAliasedILogger()
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
//                          ^^^^^^^^^^^^^^^^^^^^
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

    namespace CustomILogger
    {
        public interface ILogger { void LogError(string message); }

        public class CustomILoggerStaticEntryPoint
        {
            [FunctionName("Sample")]
            public static void CustomLoggerIsCompliant(ILogger log)
            {
                try { }
                catch { } // Compliant. log is not from Microsoft.Extensions.Logging
            }
        }

        public class CustomILoggerDependencyInjection
        {
            protected ILogger Logger() => null;

            [FunctionName("Sample")]
            public void CustomILoggerInScopeIsMustNotBeCalled()
            {
                try { }
                catch { } // Compliant.
            }
        }
    }

    namespace CustomLogger
    {
        public class CustomLogger : ILogger
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
            public bool IsEnabled(LogLevel logLevel) => true;
            public IDisposable BeginScope<TState>(TState state) => null;

            public void SomeOtherMethodOnCustomLogger() { }
            public void Log(LogLevel logLevel, string message) { }
            public void Log(string message) { }
        }

        public static class CustomLoggerExtensions
        {
            public static void LogTrace(this CustomLogger logger) { }
            public static void LogError(this CustomLogger logger) { }
            public static void SomeExtensionMethod(this CustomLogger logger) { }
        }

        public class CustomLoggerStaticEntryPoint
        {
            [FunctionName("Sample")]
            public static void CustomLoggerIsCompliant()
            {
                try { }
                catch
                {
                    var logger = new CustomLogger();
                    logger.Log(LogLevel.Error, default(EventId), state: (object)null, exception: null, formatter: (o, ex) => String.Empty); // Compliant. Some ILogger.Log method is called.
                }
            }

            [FunctionName("Sample")]
            public static void CustomLoggerIsInScopeButNotCalled()
            {
                var logger = new CustomLogger();
                try { }
                catch { } // FN. ILogger is in scope, but locals are ignored.
            }

            [FunctionName("Sample")]
            public static void CustomParameterLoggerIsInScopeButNotCalled(CustomLogger logger)
            {
                try { }
                catch { } // Noncompliant
            }

            [FunctionName("Sample")]
            public static void CustomParameterLoggerIsInScopeAndCalled(CustomLogger logger)
            {
                try { }
                catch
                {
                    logger.Log(LogLevel.Error, default(EventId), state: (object)null, exception: null, formatter: (o, ex) => String.Empty); // Compliant.
                }
            }

            [FunctionName("Sample")]
            public static void CustomParameterLoggerIsInScopeButCalledWithInsufficentLogLevel(CustomLogger logger)
            {
                try { }
                catch // Noncompliant
                {
                    logger.Log(LogLevel.Trace, default(EventId), state: (object)null, exception: null, formatter: (o, ex) => String.Empty); // Secondary
                }
            }

            [FunctionName("Sample")]
            public static void CustomParameterLoggerIsInScopeAndSomeNonInterfaceLogMethodIsCalledWithInsufficentLogLevel(CustomLogger logger)
            {
                try { }
                catch // Noncompliant
                {
                    logger.Log(LogLevel.Trace, ""); // Secondary
                }
            }

            [FunctionName("Sample")]
            public static void CustomParameterLoggerIsInScopeAndSomeNonInterfaceLogMethodIsCalledWithSufficentLogLevel(CustomLogger logger)
            {
                try { }
                catch
                {
                    logger.Log(LogLevel.Error, ""); // Compliant
                }
            }

            [FunctionName("Sample")]
            public static void CustomParameterLoggerIsInScopeAndSomeNonInterfaceLogMethodIsCalledWithoutLogLevelArgument(CustomLogger logger)
            {
                try { }
                catch
                {
                    logger.Log(""); // Compliant
                }
            }
        }

        public class CustomLoggerDependencyInjection
        {
            private readonly CustomLogger _logger;

            [FunctionName("Sample")]
            public void CustomLoggerInScopeMustBeCalled()
            {
                try { }
                catch { } // Noncompliant.
            }

            [FunctionName("Sample")]
            public void CustomLoggerLogCalled()
            {
                try { }
                catch
                {
                    _logger.Log(LogLevel.Error, default(EventId), state: (object)null, exception: null, formatter: (o, ex) => String.Empty); // Compliant.
                }
            }

            [FunctionName("Sample")]
            public void CustomLoggerLogCalledWithUnsufficientLogLevel()
            {
                try { }
                catch // Noncompliant
                {
                    _logger.Log(LogLevel.Trace, default(EventId), state: (object)null, exception: null, formatter: (o, ex) => String.Empty); // Secondary
                }
            }

            [FunctionName("Sample")]
            public void CustomLoggerOtherMethodCalled()
            {
                try { }
                catch
                {
                    _logger.SomeOtherMethodOnCustomLogger(); // Compliant. Any other call is considered as valid logging
                }
            }

            [FunctionName("Sample")]
            public void CustomLoggerNonLoggingMethodIsEnabledFromILogger()
            {
                try { }
                catch // Noncompliant
                {
                    _logger.IsEnabled(LogLevel.Error); // Secondary
                }
            }

            [FunctionName("Sample")]
            public void CustomLoggerNonLoggingMethodBeginScopeFromILogger()
            {
                try { }
                catch // Noncompliant
                {
                    _logger.BeginScope((object)null); // Secondary
                }
            }

            [FunctionName("Sample")]
            public void CustomLoggerOtherExtensionMethodCalled()
            {
                try { }
                catch
                {
                    _logger.SomeExtensionMethod(); // Compliant. Any unknown extension method is considered as valid logging
                }
            }

            [FunctionName("Sample")]
            public void CustomLoggerSpecialNamedExtensionInvalidMethodCalled()
            {
                try { }
                catch
                {
                    _logger.LogTrace(); // Special named extension methods are only considered if they are defined in Microsoft.Extensions.Logging.LoggerExtensions
                }
            }

            [FunctionName("Sample")]
            public void CustomLoggerSpecialNamedExtensionValidMethodCalled()
            {
                try { }
                catch
                {
                    _logger.LogError(); // Compliant.
                }
            }
        }
    }

    namespace ImplicitILoggerImpl
    {
        public class CustomLogger : ILogger
        {
            void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
            bool ILogger.IsEnabled(LogLevel logLevel) => true;
            IDisposable ILogger.BeginScope<TState>(TState state) => null;
        }

        public class CustomLoggerDependencyInjection
        {
            private readonly CustomLogger _logger;

            [FunctionName("Sample")]
            public void CastedToILoggerCallsLogError()
            {
                try { }
                catch
                {
                    ((ILogger)_logger).LogError(""); // Compliant.
                }
            }

            [FunctionName("Sample")]
            public void CastedToILoggerCallsLogTrace()
            {
                try { }
                catch // Noncompliant.
                {
                    ((ILogger)_logger).LogTrace(""); // Secondary
                }
            }

            [FunctionName("Sample")]
            public void CastedToILoggerCallsLogInstanceMethodWithLogLevelTrace()
            {
                try { }
                catch // Noncompliant.
                {
                    ((ILogger)_logger).Log(LogLevel.Trace, default(EventId), (object)null, default(Exception), (s, ex) => string.Empty); // Secondary
                }
            }
        }
    }
}

namespace AzureFunctionLogFailuresMissingUsingTests
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging.Abstractions;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class LoggerNotInScopeStaticFunction
    {
        [FunctionName("Sample")]
        public static void ILoggerNotImported(ILogger log) // Error [CS0246]: The type or namespace name 'ILogger'
        {
            try { }
            catch { } // Compliant
        }
    }

    public class LoggerNotInScopeInstance
    {
        private ILogger log; // Error [CS0246]: The type or namespace name 'ILogger'

        [FunctionName("Sample")]
        public void ILoggerNotImported()
        {
            try { }
            catch { } // Compliant
        }

    }
}
