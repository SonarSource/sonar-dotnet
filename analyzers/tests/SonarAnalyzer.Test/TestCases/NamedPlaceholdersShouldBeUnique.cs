using System;

namespace MicrosoftTests
{
    using Microsoft.Extensions.Logging;

    public class Program
    {
        void Compliant(ILogger logger, string foo, string bar)
        {
            var args = new object[] { foo, bar };

            Console.WriteLine("Hey {foo} and {foo}"); // Compliant

            // Do not raise on duplicate indexes
            logger.LogInformation("Hey {0} and {0}", foo, bar);             // Compliant
            logger.LogDebug("Hey {0} and {foo} and {0}", foo, foo, bar);    // Compliant
            logger.LogTrace("Hey {0} and {0}", foo, bar);                   // Compliant
            logger.LogCritical("Hey {0} and {0}", foo, bar);                // Compliant
            logger.LogWarning("Hey {0} and {0}", foo, bar);                 // Compliant
            logger.LogError("Hey {0} and {0}", foo, bar);                   // Compliant

            logger.LogInformation("Hey {foo} and {bar}", foo, bar);         // Compliant
            logger.LogDebug("Hey {foo} and {bar}", foo, bar);               // Compliant
            logger.LogTrace("Hey {foo} and {bar}", foo, bar);               // Compliant
            logger.LogCritical("Hey {foo} and {bar}", foo, bar);            // Compliant
            logger.LogWarning("Hey {foo} and {bar}", foo, bar);             // Compliant

            logger.Log(LogLevel.Trace, "Hey {foo} and {bar}", foo, bar);    // Compliant

            // Out of order
            logger.LogError(args: args, message: "Hey {foo} and {bar}");    // Compliant

            // Calling as static method
            LoggerExtensions.LogCritical(logger, "Hey {foo} and {bar}", foo, bar);                  // Compliant
            LoggerExtensions.LogDebug(message: "Hey {foo} and {bar}", logger: logger, args: args);  // Compliant

            // Custom type not implementing ILogger
            new NotLogger().LogDebug("Hey {foo} and {foo}", foo, foo);      // Compliant
            new NotLogger().LogCritical("Hey {foo} and {foo}", foo, foo);   // Compliant

        }

        void Noncompliant_Simple(ILogger logger, string foo, Exception ex, EventId eventId)
        {
            logger.LogInformation("Hey {foo} and {foo}", foo);              // Noncompliant {{Message template placeholder 'foo' is not unique.}}
            logger.LogDebug("Hey {foo} and {foo}", foo, foo);               // Noncompliant
            logger.LogTrace("Hey {foo} and {foo}", foo, foo);               // Noncompliant
            logger.LogCritical("Hey {foo} and {foo}", foo, foo);            // Noncompliant
            logger.LogWarning("Hey {foo} and {foo}", foo, foo);             // Noncompliant
            logger.LogError("Hey {foo} and {foo}", foo, foo);               // Noncompliant
            //                              ^^^
            logger.LogInformation("Hey {foo} and {foo}");                   // Noncompliant
            //                                    ^^^
            logger.Log(LogLevel.Trace, "Hey {foo} and {foo}", foo, foo);    // Noncompliant
            //                                         ^^^
            logger.LogDebug(eventId, ex, "Hey {foo} and {foo}", foo, foo);  // Noncompliant
            //                                           ^^^
        }

        void Noncompliant_Custom(MyLogger logger, string foo)
        {
            logger.LogInformation("Hey {foo} and {foo}", foo, foo);     // Noncompliant {{Message template placeholder 'foo' is not unique.}}
            logger.LogDebug("Hey {foo} and {foo}", foo, foo);           // Noncompliant
            logger.LogTrace("Hey {foo} and {foo}", foo, foo);           // Noncompliant
            logger.LogCritical("Hey {foo} and {foo}", foo, foo);        // Noncompliant
            logger.LogWarning("Hey {foo} and {foo}", foo, foo);         // Noncompliant
            logger.LogError("Hey {foo} and {foo}", foo, foo);           // Noncompliant
            //                              ^^^
        }

        void Noncompliant_Complex(ILogger logger, string foo, string bar, string baz, Exception ex, EventId eventId)
        {
            logger.LogInformation("Hey {foo} and {foo} and {foo}", foo, bar);
            //                                    ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                              ^^^ @-1 {{Message template placeholder 'foo' is not unique.}}

            logger.LogInformation("Hey {foo} and {foo} and {foo} and {bar}", foo, foo, foo, bar);
            //                                    ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                              ^^^ @-1 {{Message template placeholder 'foo' is not unique.}}

            logger.LogCritical("Hey {foo} and {bar} and {foo} and {bar}", foo, bar, foo, bar);
            //                                           ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                                     ^^^ @-1 {{Message template placeholder 'bar' is not unique.}}

            logger.LogDebug("Hey {foo} and {bar} and {foo} and {bar} and {baz} {baz}", foo, bar, foo, bar, baz, baz);
            //                                        ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                                  ^^^ @-1 {{Message template placeholder 'bar' is not unique.}}
            //                                                                  ^^^ @-2 {{Message template placeholder 'baz' is not unique.}}

            logger.LogTrace("Hey {foo} and {0} and {foo} and {bar} and {0} {baz}", foo, bar, foo, bar, baz, baz); // Noncompliant
            //                                      ^^^
            LoggerExtensions.LogWarning(logger, "Hey {foo} {foo} and {bar} and {0} {baz}", foo, foo, bar, baz, baz); // Noncompliant
            //                                              ^^^
            LoggerExtensions.LogError(message: "Hey {foo} {foo} and {bar} and {0} {baz}", logger: logger, args: Array.Empty<object>()); // Noncompliant
            //                                             ^^^
            LoggerExtensions.LogInformation(logger, args: Array.Empty<object>(), message: "Hey {foo} and {foo}"); // Noncompliant
            //                                                                                            ^^^

            logger.Log(LogLevel.Trace, args: new[] { foo, foo }, message: "Hey {foo} and {foo}"); // Noncompliant
            //                                                                            ^^^

            logger.LogDebug(
                message: "Hey {foo} and {foo}", // Noncompliant
                //                       ^^^
                eventId: eventId,
                exception: ex,
                args: new[] { foo, foo });

            LoggerExtensions.Log(
                logger,
                logLevel: LogLevel.Trace,
                args: Array.Empty<object>(),
                message: "Hey {foo} and {foo}"); // Noncompliant
            //                           ^^^

            logger.LogWarning(@"
                hey there
                {foo}
                and {bar}
                and again {foo}
                ", foo, bar, foo);
            //             ^^^ @-1 {{Message template placeholder 'foo' is not unique.}}
        }

        // Real
        class MyLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        }

        // Fake
        public class NotLogger
        {
            public void LogDebug(string message, params object[] args) { }
        }
    }

    public static class NotLoggerExtensions
    {
        public static void LogCritical(this Program.NotLogger logger, string message, params object[] args) { }
    }
}

namespace SerilogTests
{
    using Serilog;
    using Serilog.Events;

    public class Program
    {
        void Compliant(string foo, string bar, Exception e, LogEventLevel level)
        {
            Log.Verbose("Hey {foo} and {bar}", foo, bar);           // Compliant
            Log.Debug("Hey {foo} and {bar}", foo, bar);             // Compliant
            Log.Information("Hey {foo} and {bar}", foo, bar);       // Compliant
            Log.Warning("Hey {foo} and {bar}", foo, bar);           // Compliant
            Log.Error("Hey {foo} and {bar}", foo, bar);             // Compliant
            Log.Fatal("Hey {foo} and {bar}", foo, bar);             // Compliant
            Log.Write(level, e, "Hey {foo} and {bar}", foo, bar);   // Compliant
        }

        void Noncompliant_Simple(string foo, string bar, Exception e, LogEventLevel level)
        {
            Log.Verbose("Hey {foo} and {foo}", foo, bar);           // Noncompliant
            Log.Debug("Hey {foo} and {foo}", foo, bar);             // Noncompliant
            Log.Information("Hey {foo} and {foo}", foo, bar);       // Noncompliant
            Log.Warning("Hey {foo} and {foo}", foo, bar);           // Noncompliant
            Log.Error("Hey {foo} and {foo}", foo, bar);             // Noncompliant
            Log.Fatal("Hey {foo} and {foo}", foo, bar);             // Noncompliant
            Log.Write(level, e, "Hey {foo} and {foo}", foo, bar);   // Noncompliant
        }
    }
}

namespace NLogTests
{
    using NLog;

    public class Program
    {
        void Compliant(ILogger logger, string foo, string bar)
        {
            logger.Info("Hey {foo} and {bar}", foo, bar);                           // Compliant
            logger.Debug("Hey {foo} and {bar}", foo, bar);                          // Compliant
            logger.Trace("Hey {foo} and {bar}", foo, bar);                          // Compliant
            logger.Error("Hey {foo} and {bar}", foo, bar);                          // Compliant
            logger.Fatal("Hey {foo} and {bar}", foo, bar);                          // Compliant
            logger.Warn("Hey {foo} and {bar}", foo, bar);                           // Compliant
            logger.ConditionalTrace("Hey {foo} and {bar}", foo, bar);               // Compliant
            logger.ConditionalDebug("Hey {foo} and {bar}", foo, bar);               // Compliant
            logger.Log(LogLevel.Trace, "Hey {foo} and {bar}", foo, bar);            // Compliant

            new MyLogger().Log(LogLevel.Trace, "Hey {foo} and {bar}", foo, foo);    // Compliant
            new MyLogger().Trace("Hey {foo} and {bar}", foo, foo);                  // Compliant
        }

        void Noncompliant(ILogger interfaceLogger, Logger classLogger, string foo)
        {
            classLogger.Info("Hey {foo} and {foo}", foo, foo);                      // Noncompliant {{Message template placeholder 'foo' is not unique.}}
            classLogger.Debug("Hey {foo} and {foo}", foo, foo);                     // Noncompliant
            classLogger.Trace("Hey {foo} and {foo}", foo, foo);                     // Noncompliant
            classLogger.Error("Hey {foo} and {foo}", foo, foo);                     // Noncompliant
            classLogger.Fatal("Hey {foo} and {foo}", foo, foo);                     // Noncompliant
            classLogger.Warn("Hey {foo} and {foo}", foo, foo);                      // Noncompliant
            classLogger.ConditionalTrace("Hey {foo} and {foo}", foo, foo);          // Noncompliant
            classLogger.ConditionalDebug("Hey {foo} and {foo}", foo, foo);          // Noncompliant
            classLogger.Log(LogLevel.Trace, "Hey {foo} and {foo}", foo, foo);       // Noncompliant

            interfaceLogger.Trace("Hey {foo} and {foo}", foo, foo);                 // Noncompliant
            interfaceLogger.Log(LogLevel.Trace, "Hey {foo} and {foo}", foo, foo);   // Noncompliant
            //                                                  ^^^

            new MyLogger().Log(LogLevel.Trace, "Hey {foo} and {foo}", foo, foo);    // Noncompliant
            new MyLogger().Trace("Hey {foo} and {foo}", foo, foo);                  // Noncompliant

        }

        public class MyLogger : Logger { }
    }
}
