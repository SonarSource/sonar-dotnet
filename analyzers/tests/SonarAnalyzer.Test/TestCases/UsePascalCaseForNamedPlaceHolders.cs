using System;
using Microsoft.Extensions.Logging;

public class Program
{
    public void Basics(ILogger logger, int arg)
    {
        logger.LogInformation("No placeholder");                    // Compliant
        logger.LogInformation("Arg: {Arg}", arg);
        logger.LogInformation("Arg: {0}", arg);
        logger.LogInformation("Arg: {_}", arg);
        logger.LogInformation("Arg: {_name}", arg);
        logger.LogInformation("Arg: {_Name}", arg);
        logger.LogInformation("Arg: {}", arg);
        logger.LogInformation("Arg: {{arg}}", arg);
        logger.LogInformation("Arg: {Arg} {Arg}", arg, arg);
        logger.LogInformation("Arg: {Arg1} {Arg2}", arg, arg);

        logger.BeginScope("scope");
        logger.BeginScope("{arg}", arg);

        logger.LogInformation("Arg: {arg}", arg);
        //                    ^^^^^^^^^^^^ {{Use PascalCase for named placeholders.}}
        //                           ^^^ Secondary @-1

        logger.LogInformation("Arg: {Arg} {arg}", arg, arg);
        //                    ^^^^^^^^^^^^^^^^^^
        //                                 ^^^ Secondary @-1

        logger.LogInformation("Arg: {arg} {Arg}", arg, arg);
        //                    ^^^^^^^^^^^^^^^^^^
        //                           ^^^ Secondary @-1

        logger.LogInformation("Arg: {arg} {arg}", arg, arg);
        //                    ^^^^^^^^^^^^^^^^^^
        //                           ^^^ Secondary @-1
        //                                 ^^^ Secondary @-2

        logger.LogInformation(@"
             Arg: {arg}
             {arg}", arg, arg);
        // Noncompliant @-3 ^31#46
        // Secondary @-3    ^20#3
        // Secondary @-3    ^15#3

        LoggerExtensions.LogInformation(logger, "Arg: {arg}", arg); // Noncompliant
                                                                    // Secondary @-1

        logger.LogInformation("Arg: {Argumentvalue}", arg);         // FN - should be {ArgumentValue}, but the analyzer doesn't use any kind of word dictionary, it only checks the first character
    }

    public void NamedArguments(ILogger logger, int arg)
    {
        logger.LogInformation(args: new object[] { arg }, message: "Arg: {Arg}");   // Compliant
        logger.LogInformation(message: "Arg: {Arg}", args: new object[] { arg });   // Compliant
        logger.LogInformation(args: new object[] { arg }, message: "Arg: {arg}");   // Noncompliant
                                                                                    // Secondary @-1
        logger.LogInformation(message: "Arg: {arg}", args: new object[] { arg });   // Noncompliant
                                                                                    // Secondary @-1
    }

    public void IncorrectPlaceholderFormat(ILogger logger, int arg)
    {
        logger.LogInformation("Arg: {@arg}", arg);      // Noncompliant
                                                        // Secondary @-1
        logger.LogInformation("Arg: {&arg}", arg);
        logger.LogInformation("Arg: {arg,23}", arg);    // Noncompliant
                                                        // Secondary @-1
        logger.LogInformation("Arg: {arg,arg}", arg);
    }

    public void ClassImplementsILogger(CustomLogger logger, int arg)
    {
        logger.LogCritical("Arg: {arg}", arg);                      // Noncompliant
                                                                    // Secondary @-1
        logger.LogInformation("Arg: {arg}", arg);                   // Noncompliant
                                                                    // Secondary @-1
    }

    public void ClassDoesNotImplementILogger(NotILogger notILogger, int arg)
    {
        notILogger.LogInformation("Arg: {arg}", arg);
        notILogger.LogCritical("Arg: {arg}", arg);
    }

    public void Interpolation(ILogger logger, int arg)
    {
        logger.LogInformation($"Arg: {arg}");                       // Compliant
        logger.LogInformation($"Arg: {{arg}}", arg);                // Noncompliant
                                                                    // Secondary @-1
        logger.LogInformation($"{arg}: {{arg}}", arg);
        //                    ^^^^^^^^^^^^^^^^^
        //                               ^^^                           Secondary @-1
        logger.LogInformation("Arg: " + $"{{arg}}", arg);           // FN
        logger.LogInformation("Arg: " + $"{arg} {{arg}}", arg);     // FN
    }

    public class CustomLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }

    public class NotILogger
    {
        public void LogCritical(string message, params object[] args) { }
        public void LogInformation(string message, params object[] args) { }
    }

    public static class NotILoggerExtensions
    {
        public static void LogCritical(NotILogger logger, string message, params object[] args) { }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/9545
    public class Repro_9545
    {
        public void Method(ILogger logger, int number)
        {
            logger.LogDebug($"{nameof(Repro_9545)} filter: {{number}}", number);
            //              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //                                               ^^^^^^                    Secondary @-1
            logger.LogDebug($"Repro_9545 filter: {{number}}", number);
            //              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //                                     ^^^^^^                              Secondary @-1
        }
    }
}
