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

        logger.LogInformation("Arg: {arg}", arg);                   // Noncompliant {{Use PascalCase for named placeholders.}}
        //                           ^^^
        logger.LogInformation("Arg: {Arg} {arg}", arg, arg);        // Noncompliant
        //                                 ^^^
        logger.LogInformation("Arg: {arg} {Arg}", arg, arg);        // Noncompliant
        //                           ^^^
        logger.LogInformation("Arg: {arg} {Arg}", arg, arg);        // Noncompliant
        //                           ^^^
        logger.LogInformation("Arg: {arg} {arg}", arg, arg);
        //                           ^^^
        //                                 ^^^ @-1
        logger.LogInformation(@"
             Arg: {arg}
             {arg}", arg, arg);
        //         ^^^ @-1
        //    ^^^ @-1

        LoggerExtensions.LogInformation(logger, "Arg: {arg}", arg); // Noncompliant

        logger.LogInformation("Arg: {Argumentvalue}", arg);         // FN - should be {ArgumentValue}, but the analyzer doesn't use any kind of word dictionary, it only checks the first character
    }

    public void NamedArguments(ILogger logger, int arg)
    {
        logger.LogInformation(args: new object[] { arg }, message: "Arg: {Arg}");   // Compliant
        logger.LogInformation(message: "Arg: {Arg}", args: new object[] { arg });   // Compliant
        logger.LogInformation(args: new object[] { arg }, message: "Arg: {arg}");   // Noncompliant
        logger.LogInformation(message: "Arg: {arg}", args: new object[] { arg });   // Noncompliant
    }

    public void IncorrectPlaceholderFormat(ILogger logger, int arg)
    {
        logger.LogInformation("Arg: {@arg}", arg);      // Noncompliant
        logger.LogInformation("Arg: {&arg}", arg);
        logger.LogInformation("Arg: {arg,23}", arg);    // Noncompliant
        logger.LogInformation("Arg: {arg,arg}", arg);
    }

    public void ClassImplementsILogger(CustomLogger logger, int arg)
    {
        logger.LogCritical("Arg: {arg}", arg);                      // Noncompliant
        logger.LogInformation("Arg: {arg}", arg);                   // Noncompliant
    }

    public void ClassDoesNotImplementILogger(NotILogger notILogger, int arg)
    {
        notILogger.LogInformation("Arg: {arg}", arg);
        notILogger.LogCritical("Arg: {arg}", arg);
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
}
