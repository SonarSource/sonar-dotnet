using System;

namespace MicrosoftTests
{
    using Microsoft.Extensions.Logging;

    public class Program
    {
        void Compliant(ILogger logger, string foo, string bar)
        {
            var args = new object[] { foo, bar };

            Console.WriteLine("Hey {foo} and {foo}");                       // Compliant

            // Do not raise on duplicate indexes
            logger.LogDebug("Hey {0} and {foo} and {0}", foo, foo, bar);    // Compliant
            logger.LogTrace("Hey {0} and {0}", foo, bar);                   // Compliant

            // Do not raise on wildcard placeholder
            logger.LogError("Hey {_} and {_}", foo, bar);                   // Compliant
            logger.LogError("Hey {_} and {_0}", foo, bar);                  // Compliant

            logger.Log(LogLevel.Trace, "Hey {foo} and {bar}", foo, bar);    // Compliant

            // Out of order
            logger.LogError(args: args, message: "Hey {foo} and {bar}");    // Compliant

            // Calling as static method
            LoggerExtensions.LogCritical(logger, "Hey {foo} and {bar}", foo, bar);                  // Compliant
            LoggerExtensions.LogDebug(message: "Hey {foo} and {bar}", logger: logger, args: args);  // Compliant

            // Custom type not implementing ILogger
            new NotLogger().LogDebug("Hey {foo} and {foo}", foo, foo);      // Compliant
            new NotLogger().LogCritical("Hey {foo} and {foo}", foo, foo);   // Compliant

            // Invalid template syntaxs
            logger.LogInformation("Hey foo} and {foo}", foo, foo);           // Compliant
            logger.LogInformation("Hey {foo and {foo}", foo, foo);           // Compliant
            logger.LogInformation("Hey {foo} and {foo", foo, foo);           // Compliant
            logger.LogInformation("Hey {foo} and {&foo}", foo);              // Compliant
            logger.LogInformation("Hey {foo} and {@foo,INVALID}", foo);      // Compliant
            logger.LogInformation("Hey {foo} and {@foo#}", foo);             // Compliant

            // Escaped
            logger.LogInformation("Hey {{foo}} and {{foo}}", foo, foo);      // Compliant
            logger.LogInformation("Hey {foo} and {{foo}}", foo, foo);        // Compliant

            // Contains Interpolation
            logger.LogWarning($"Hey {foo} and {foo}", foo, foo);                    // Compliant
            logger.LogWarning($"Hey {foo}" + "and {foo}", foo, foo);                // Compliant
            logger.LogInformation("Hey {foo}" + $"and {foo}" + "{foo}", foo, foo);  // FN due to interpolation being ignored in complex cases

            // Contains Identifier name
            logger.LogWarning("Hey {" + foo + "} and {" + foo + "}");        // Compliant

            logger.LogInformation($"Hey {{foo}} and {{foo}}", foo, foo);     // Noncompliant
                                                                             // Secondary @-1
            logger.LogInformation("Hey" + "{" + "foo} and {foo" +"}");       // FN

            logger.LogInformation(true ? "Hey {foo}" : "Hi {foo}", foo);     // Compliant
        }

        void Noncompliant(ILogger logger, string foo, string bar, string baz, Exception ex, EventId eventId)
        {
            logger.Log(LogLevel.Trace, "Hey {foo} and {foo}", foo, bar);
            //                               ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                         ^^^ Secondary @-1 {{Message template placeholder 'foo' is not unique.}}

            logger.LogInformation("Hey {foo} and {foo} and {foo}", foo, bar);
            //                          ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                    ^^^ Secondary @-1
            //                                              ^^^ Secondary @-2

            logger.LogInformation("Hey {foo} and {foo} and {foo} and {bar}", foo, foo, foo, bar);
            //                          ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                    ^^^ Secondary @-1 {{Message template placeholder 'foo' is not unique.}}
            //                                              ^^^ Secondary @-2 {{Message template placeholder 'foo' is not unique.}}

            logger.LogCritical("Hey {foo} and {bar} and {foo} and {bar}", foo, bar, foo, bar);
            //                       ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                           ^^^ Secondary @-1 {{Message template placeholder 'foo' is not unique.}}
            //                                 ^^^ @-2 {{Message template placeholder 'bar' is not unique.}}
            //                                                     ^^^ Secondary @-3 {{Message template placeholder 'bar' is not unique.}}

            logger.LogDebug("Hey {foo} and {bar} and {foo} and {bar} and {baz} {baz}", foo, bar, foo, bar, baz, baz);
            //                    ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                        ^^^ Secondary @-1
            //                              ^^^ @-2 {{Message template placeholder 'bar' is not unique.}}
            //                                                  ^^^ Secondary @-3
            //                                                            ^^^ @-4 {{Message template placeholder 'baz' is not unique.}}
            //                                                                  ^^^ Secondary @-5

            logger.LogTrace("Hey {foo} and {0} and {foo} and {bar} and {0} {baz}", foo, bar, foo, bar, baz, baz);
            //                    ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                      ^^^ Secondary @-1

            LoggerExtensions.LogWarning(logger, "Hey {foo} {foo} and {bar} and {0} {baz}", foo, foo, bar, baz, baz); // Noncompliant
                                                                                                                     // Secondary @-1
            LoggerExtensions.LogError(message: "Hey {foo} {foo} and {bar} and {0} {baz}", logger: logger, args: Array.Empty<object>()); // Noncompliant
                                                                                                                                        // Secondary @-1

            LoggerExtensions.LogInformation(logger, args: Array.Empty<object>(), message: "Hey {foo} and {foo}"); // Noncompliant
                                                                                                                  // Secondary @-1

            logger.Log(LogLevel.Trace, args: new[] { foo, foo }, message: "Hey {foo} and {foo}"); // Noncompliant
                                                                                                  // Secondary @-1

            // Grammar checks
            logger.LogInformation("Hey {foo} and {$foo} and {@foo}", foo, bar);
            //                          ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                     ^^^ Secondary @-1
            //                                                ^^^ Secondary @-2

            logger.LogInformation("Hey {foo} and {foo,42} and {foo:format} and {foo,-42:for_{_mat}", foo, bar);
            //                          ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                    ^^^ Secondary @-1
            //                                                 ^^^ Secondary @-2
            //                                                                  ^^^ Secondary @-3

            logger.LogInformation("Hey {_foo} and {_foo42} and {_foo42} and {_foo}", foo, bar);
            //                          ^^^^ {{Message template placeholder '_foo' is not unique.}}
            //                                                               ^^^^ Secondary @-1
            //                                     ^^^^^^ @-2 {{Message template placeholder '_foo42' is not unique.}}
            //                                                  ^^^^^^ Secondary @-3

            // Multiline
            logger.LogDebug(
                message: "Hey {foo} and {foo}",
            //                 ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                           ^^^ Secondary @-1
                eventId: eventId,
                exception: ex,
                args: new[] { foo, foo });

            LoggerExtensions.Log(
                logger,
                logLevel: LogLevel.Trace,
                args: Array.Empty<object>(),
                message: "Hey {foo} and {foo}"); // Noncompliant
                                                 // Secondary @-1

            logger.LogWarning(@"
                hey there
                {foo}
                and {bar}
                and again {foo}
                ",
            //   ^^^ @-3 {{Message template placeholder 'foo' is not unique.}}
            //             ^^^ Secondary @-2
                foo, bar, foo);


            // SyntaxKinds
            logger.LogInformation("Hey {foo}" + "and {foo}", foo, foo);
            //                          ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                        ^^^ Secondary @-1
            logger.LogInformation("Hey {foo}" + $"and of course" + "{foo}", foo, foo);
            //                          ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                                       ^^^ Secondary @-1
            logger.LogInformation("Hey {foo}" + $"{{foo}}" + "{foo}", foo, foo);                    // We miss the second placeholder
            //                          ^^^ {{Message template placeholder 'foo' is not unique.}}
            //                                                 ^^^ Secondary @-1

            logger.LogInformation(
                "Hey {foo}"
            //        ^^^ {{Message template placeholder 'foo' is not unique.}}
                +
                "and {foo}", foo, foo);
            //        ^^^ Secondary

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
        void Compliant(ILogger logger, string arg)
        {
            logger.Write(LogEventLevel.Information, "Hey {foo} and {bar}");                // Compliant
            logger.Write(LogEventLevel.Information, "Hey {foo} and {bar}", arg);           // Compliant
            logger.Write(LogEventLevel.Information, "Hey {foo} and {bar}", arg, arg);      // Compliant
        }

        void Noncompliant(ILogger logger, string arg)
        {
            logger.Write(LogEventLevel.Information, "Hey {foo} and {foo}");                // Noncompliant
                                                                                           // Secondary @-1
            logger.Write(LogEventLevel.Information, "Hey {foo} and {foo}", arg);           // Noncompliant
                                                                                           // Secondary @-1
            logger.Write(LogEventLevel.Information, "Hey {foo} and {foo}", arg, arg);      // Noncompliant
                                                                                           // Secondary @-1
        }
    }
}

namespace NLogTests
{
    using NLog;

    public class Program
    {
        void Compliant(ILoggerBase logger, string arg)
        {
            logger.Log(LogLevel.Trace, "Hey {foo} and {bar}");              // Compliant
            logger.Log(LogLevel.Trace, "Hey {foo} and {bar}", arg);         // Compliant
            logger.Log(LogLevel.Trace, "Hey {foo} and {bar}", arg, arg);    // Compliant
        }

        void Noncompliant(ILoggerBase logger, string arg)
        {
            logger.Log(LogLevel.Trace, "Hey {foo} and {foo}");              // Noncompliant
                                                                            // Secondary @-1
            logger.Log(LogLevel.Trace, "Hey {foo} and {foo}", arg);         // Noncompliant
                                                                            // Secondary @-1
            logger.Log(LogLevel.Trace, "Hey {foo} and {foo}", arg, arg);    // Noncompliant
                                                                            // Secondary @-1
        }
    }
}
