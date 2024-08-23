using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using AliasedLogger = Microsoft.Extensions.Logging.ILogger;

public class Program
{
    private const string FieldConstant = "field";
    private string _stringField = "";
    private string StringProperty => "";
    private string StringMethod() => "";

    public void BasicScenarios(ILogger logger, int arg)
    {
        const string localConstant = "local";
        string localVariable = "";

        logger.Log(LogLevel.Warning, "");                                       // Compliant
        logger.Log(LogLevel.Warning, "", 42);                                   // Compliant - this rule doesn't care whether the additional arguments are properly used
        logger.Log(LogLevel.Warning, "{Arg}", arg);                             // Compliant
        logger.Log(LogLevel.Warning, localVariable);                            // Compliant
        logger.Log(LogLevel.Warning, _stringField);                             // Compliant
        logger.Log(LogLevel.Warning, StringProperty, 42);                       // Compliant
        logger.Log(LogLevel.Warning, StringMethod(), 42);                       // Compliant
        logger.Log(message: "{Param}", logLevel: LogLevel.Warning, args: 42);   // Compliant

        logger.Log(LogLevel.Warning, $"{arg}");                                 // Noncompliant {{Don't use string interpolation in logging message templates.}}
        //                           ^^^^^^^^

        logger.Log(LogLevel.Warning, "Argument: " + arg);                       // Noncompliant {{Don't use string concatenation in logging message templates.}}
        //                           ^^^^^^^^^^^^^^^^^^

        logger.Log(LogLevel.Warning, string.Format("{0}", arg));                // Noncompliant {{Don't use String.Format in logging message templates.}}
        //                           ^^^^^^^^^^^^^^^^^^^^^^^^^

        logger.Log(message: $"{arg}", logLevel: LogLevel.Warning);              // Noncompliant
        logger.Log(message: (string)$"{arg}", logLevel: LogLevel.Warning);      // Noncompliant
        //                          ^^^^^^^^
        logger.Log(LogLevel.Warning, (arg + " " + arg).ToLower());              // Noncompliant
        //                            ^^^^^^^^^^^^^^^

        logger.Log(LogLevel.Warning, "First " + "Second " + "Third");                               // Compliant - all strings in the concatenation are constants, the compiler can optimize it
        logger.Log(LogLevel.Warning, FieldConstant + localConstant);                                // Compliant
        logger.Log(LogLevel.Warning, FieldConstant + "Second");                                     // Compliant
        logger.Log(LogLevel.Warning, $"Constant: {FieldConstant}");                                 // Compliant, see https://github.com/SonarSource/sonar-dotnet/issues/9247
        logger.Log(LogLevel.Warning, $"Constant: {FieldConstant}" + $"Constant: {FieldConstant}");  // Compliant, see https://github.com/SonarSource/sonar-dotnet/issues/9653
        logger.Log(LogLevel.Warning, FieldConstant + $"Constant: {FieldConstant}");                 // Compliant
        logger.Log(LogLevel.Warning, "First" + $"Constant: {FieldConstant}");                       // Compliant
        logger.Log(LogLevel.Warning, "First " + arg + "Third");                                     // Noncompliant
        logger.Log(LogLevel.Warning, ("First " + "Second").ToLower());                              // FN

        logger.Log(LogLevel.Warning, new EventId(42), $"{arg}");                // Noncompliant
        logger.Log(LogLevel.Warning, new Exception(), $"{arg}");                // Noncompliant
        logger.Log(LogLevel.Warning, new Exception(), "{Arg}", arg);            // Compliant

        LoggerExtensions.Log(logger, LogLevel.Warning, "{Arg}", arg);           // Compliant
        LoggerExtensions.Log(logger, LogLevel.Warning, $"{arg}");               // Noncompliant
    }

    public void NotLoggingMethod(ILogger logger, int arg)
    {
        logger.BeginScope($"{arg}", 1, 2, 3);                                   // Compliant - not a log method, the message argument is always evaluated
    }

    public void ImplementsILogger(NullLogger nullLogger, CustomLogger customLogger, int arg)
    {
        nullLogger.Log(LogLevel.Warning, "Arg: {Arg}", arg);                    // Compliant
        nullLogger.Log(LogLevel.Warning, $"Arg: {arg}");                        // Noncompliant
        customLogger.Log(LogLevel.Warning, "Arg: {Arg}", arg);                  // Compliant
        customLogger.Log(LogLevel.Warning, $"Arg: {arg}");                      // Noncompliant
    }

    public void DoesNotImplementILogger(NotILogger notILogger, int arg)
    {
        notILogger.Log(LogLevel.Warning, "Arg: {Arg}", arg);                    // Compliant
        notILogger.Log(LogLevel.Warning, $"Arg: {arg}");                        // Compliant
    }

    public void AliasedILogger(AliasedLogger aliasedLogger, int arg)
    {
        aliasedLogger.Log(LogLevel.Warning, "Arg: {Arg}", arg);                 // Compliant
        aliasedLogger.Log(LogLevel.Warning, $"Arg: {arg}");                     // Noncompliant
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
        public void Log(LogLevel logLevel, string message, params object[] args)
        {
        }
    }
}
