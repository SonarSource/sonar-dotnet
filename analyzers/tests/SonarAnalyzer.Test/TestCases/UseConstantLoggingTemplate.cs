using System;
using NLog;
using log4net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;                            // shadows NLog.ILogger in MEL-focused test cases
using LogLevel = Microsoft.Extensions.Logging.LogLevel;                          // shadows NLog.LogLevel
using NullLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger;        // shadows NLog.NullLogger
using AliasedLogger = Microsoft.Extensions.Logging.ILogger;
using CastleLogger = Castle.Core.Logging.ILogger;
using NLogLogger = NLog.ILogger;
using NLogLoggerBase = NLog.ILoggerBase;
using NLogNullLogger = NLog.NullLogger;
using SerilogLogger = Serilog.ILogger;
using SerilogLog = Serilog.Log;

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

public class MicrosoftExtensionsLoggingTestCases
{
    public void ExtensionMethods(ILogger logger, int arg)
    {
        logger.Log(LogLevel.Warning, "Message");            // Compliant
        logger.Log(LogLevel.Warning, $"{arg}");             // Noncompliant

        logger.LogCritical("Message");                      // Compliant
        logger.LogCritical($"{arg}");                       // Noncompliant

        logger.LogDebug("Message");                         // Compliant
        logger.LogDebug($"{arg}");                          // Noncompliant

        logger.LogError("Message");                         // Compliant
        logger.LogError($"{arg}");                          // Noncompliant

        logger.LogInformation("Message");                   // Compliant
        logger.LogInformation($"{arg}");                    // Noncompliant

        logger.LogTrace("Message");                         // Compliant
        logger.LogTrace($"{arg}");                          // Noncompliant

        logger.LogWarning("Message");                       // Compliant
        logger.LogWarning($"{arg}");                        // Noncompliant
    }
}

public class Log4NetTestCases
{
    public void ILogBasicMethods(ILog logger, int arg, Exception ex)
    {
        logger.Debug("Message");                           // Compliant
        logger.Debug($"{arg}");                            // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Debug("Arg: " + arg);                       // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Debug(string.Format("{0}", arg));           // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Debug($"{arg}", ex);                        // Compliant - exception overload is excluded

        logger.Error("Message");                           // Compliant
        logger.Error($"{arg}");                            // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Error("Arg: " + arg);                       // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Error(string.Format("{0}", arg));           // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Error($"{arg}", ex);                        // Compliant - exception overload is excluded

        logger.Fatal("Message");                           // Compliant
        logger.Fatal($"{arg}");                            // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Fatal("Arg: " + arg);                       // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Fatal(string.Format("{0}", arg));           // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Fatal($"{arg}", ex);                        // Compliant - exception overload is excluded

        logger.Info("Message");                            // Compliant
        logger.Info($"{arg}");                             // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Info("Arg: " + arg);                        // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Info(string.Format("{0}", arg));            // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Info($"{arg}", ex);                         // Compliant - exception overload is excluded

        logger.Warn("Message");                            // Compliant
        logger.Warn($"{arg}");                             // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Warn("Arg: " + arg);                        // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Warn(string.Format("{0}", arg));            // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Warn($"{arg}", ex);                         // Compliant - exception overload is excluded
    }

    public void ILogFormatMethods(ILog logger, int arg)
    {
        logger.DebugFormat("Arg: {0}", arg);               // Compliant
        logger.DebugFormat($"{arg}");                      // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.DebugFormat("Arg: " + arg);                 // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.DebugFormat(string.Format("{0}", arg));     // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.ErrorFormat("Arg: {0}", arg);               // Compliant
        logger.ErrorFormat($"{arg}");                      // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.ErrorFormat("Arg: " + arg);                 // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.ErrorFormat(string.Format("{0}", arg));     // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.FatalFormat("Arg: {0}", arg);               // Compliant
        logger.FatalFormat($"{arg}");                      // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.FatalFormat("Arg: " + arg);                 // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.FatalFormat(string.Format("{0}", arg));     // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.InfoFormat("Arg: {0}", arg);                // Compliant
        logger.InfoFormat($"{arg}");                       // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.InfoFormat("Arg: " + arg);                  // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.InfoFormat(string.Format("{0}", arg));      // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.WarnFormat("Arg: {0}", arg);                // Compliant
        logger.WarnFormat($"{arg}");                       // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.WarnFormat("Arg: " + arg);                  // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.WarnFormat(string.Format("{0}", arg));      // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
    }
}

public class NLogTestCases
{
    public void LoggerMethods(NLogLogger logger, int arg)
    {
        logger.ConditionalDebug("Message");                 // Compliant
        logger.ConditionalDebug($"{arg}");                  // Noncompliant

        logger.ConditionalTrace("Message");                 // Compliant
        logger.ConditionalTrace($"{arg}");                  // Noncompliant

        logger.Debug("Message");                            // Compliant
        logger.Debug($"{arg}");                             // Noncompliant

        logger.Error("Message");                            // Compliant
        logger.Error($"{arg}");                             // Noncompliant

        logger.Fatal("Message");                            // Compliant
        logger.Fatal($"{arg}");                             // Noncompliant

        logger.Info("Message");                             // Compliant
        logger.Info($"{arg}");                              // Noncompliant

        logger.Log(NLog.LogLevel.Warn, "Message");          // Compliant
        logger.Log(NLog.LogLevel.Warn, $"{arg}");           // Noncompliant

        logger.Trace("Message");                            // Compliant
        logger.Trace($"{arg}");                             // Noncompliant

        logger.Warn("Message");                             // Compliant
        logger.Warn($"{arg}");                              // Noncompliant
    }

    public void AdditionalLoggers(NLogLoggerBase loggerBase, NLogNullLogger nullLogger, int arg)
    {
        loggerBase.Log(NLog.LogLevel.Warn, "Message");      // Compliant
        loggerBase.Log(NLog.LogLevel.Warn, $"{arg}");       // Noncompliant

        nullLogger.Log(NLog.LogLevel.Warn, "Message");      // Compliant
        nullLogger.Log(NLog.LogLevel.Warn, $"{arg}");       // Compliant - FN: NLog.NullLogger.Log resolves to NLog.Logger which is not in LoggerTypes
    }
}

public class CastleCoreTestCases
{
    public void ILoggerBasicMethods(CastleLogger logger, int arg)
    {
        logger.Debug("Message");                           // Compliant
        logger.Debug($"{arg}");                            // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Debug("Arg: " + arg);                       // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Debug(string.Format("{0}", arg));           // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.Error("Message");                           // Compliant
        logger.Error($"{arg}");                            // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Error("Arg: " + arg);                       // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Error(string.Format("{0}", arg));           // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.Fatal("Message");                           // Compliant
        logger.Fatal($"{arg}");                            // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Fatal("Arg: " + arg);                       // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Fatal(string.Format("{0}", arg));           // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.Info("Message");                            // Compliant
        logger.Info($"{arg}");                             // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Info("Arg: " + arg);                        // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Info(string.Format("{0}", arg));            // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.Trace("Message");                           // Compliant
        logger.Trace($"{arg}");                            // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Trace("Arg: " + arg);                       // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Trace(string.Format("{0}", arg));           // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.Warn("Message");                            // Compliant
        logger.Warn($"{arg}");                             // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Warn("Arg: " + arg);                        // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.Warn(string.Format("{0}", arg));            // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
    }

    public void ILoggerFormatMethods(CastleLogger logger, int arg)
    {
        logger.DebugFormat("{Arg}", arg);                   // Compliant
        logger.DebugFormat($"{arg}");                       // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.DebugFormat("Arg: " + arg);                  // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.DebugFormat(string.Format("{0}", arg));      // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.ErrorFormat("{Arg}", arg);                   // Compliant
        logger.ErrorFormat($"{arg}");                       // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.ErrorFormat("Arg: " + arg);                  // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.ErrorFormat(string.Format("{0}", arg));      // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.FatalFormat("{Arg}", arg);                   // Compliant
        logger.FatalFormat($"{arg}");                       // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.FatalFormat("Arg: " + arg);                  // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.FatalFormat(string.Format("{0}", arg));      // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.InfoFormat("{Arg}", arg);                    // Compliant
        logger.InfoFormat($"{arg}");                        // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.InfoFormat("Arg: " + arg);                   // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.InfoFormat(string.Format("{0}", arg));       // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.TraceFormat("{Arg}", arg);                   // Compliant
        logger.TraceFormat($"{arg}");                       // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.TraceFormat("Arg: " + arg);                  // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.TraceFormat(string.Format("{0}", arg));      // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}

        logger.WarnFormat("{Arg}", arg);                    // Compliant
        logger.WarnFormat($"{arg}");                        // Noncompliant {{Don't use string interpolation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.WarnFormat("Arg: " + arg);                   // Noncompliant {{Don't use string concatenation in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
        logger.WarnFormat(string.Format("{0}", arg));       // Noncompliant {{Don't use String.Format in logging message templates. Use a format method (e.g. 'DebugFormat') with indexed placeholders instead.}}
    }
}

public class SerilogTestCases
{
    public void InstanceMethods(SerilogLogger logger, int arg)
    {
        logger.Debug("Message without argument");                       // Compliant
        logger.Debug("The argument is {@Argument}", arg);               // Compliant
        logger.Debug($"The argument is {arg}");                         // Noncompliant

        logger.Error("Message without argument");                       // Compliant
        logger.Error("The argument is {@Argument}", arg);               // Compliant
        logger.Error($"The argument is {arg}");                         // Noncompliant

        logger.Fatal("Message without argument");                       // Compliant
        logger.Fatal("The argument is {@Argument}", arg);               // Compliant
        logger.Fatal($"The argument is {arg}");                         // Noncompliant

        logger.Information("Message without argument");                 // Compliant
        logger.Information("The argument is {@Argument}", arg);         // Compliant
        logger.Information($"The argument is {arg}");                   // Noncompliant

        logger.Verbose("Message without argument");                     // Compliant
        logger.Verbose("The argument is {@Argument}", arg);             // Compliant
        logger.Verbose($"The argument is {arg}");                       // Noncompliant

        logger.Warning("Message without argument");                     // Compliant
        logger.Warning("The argument is {@Argument}", arg);             // Compliant
        logger.Warning($"The argument is {arg}");                       // Noncompliant
    }

    public void StaticMethods(int arg)
    {
        SerilogLog.Debug("Message without argument");                   // Compliant
        SerilogLog.Debug("The argument is {@Argument}", arg);           // Compliant
        SerilogLog.Debug($"The argument is {arg}");                     // Noncompliant

        SerilogLog.Error("Message without argument");                   // Compliant
        SerilogLog.Error("The argument is {@Argument}", arg);           // Compliant
        SerilogLog.Error($"The argument is {arg}");                     // Noncompliant

        SerilogLog.Fatal("Message without argument");                   // Compliant
        SerilogLog.Fatal("The argument is {@Argument}", arg);           // Compliant
        SerilogLog.Fatal($"The argument is {arg}");                     // Noncompliant

        SerilogLog.Information("Message without argument");             // Compliant
        SerilogLog.Information("The argument is {@Argument}", arg);     // Compliant
        SerilogLog.Information($"The argument is {arg}");               // Noncompliant

        SerilogLog.Verbose("Message without argument");                 // Compliant
        SerilogLog.Verbose("The argument is {@Argument}", arg);         // Compliant
        SerilogLog.Verbose($"The argument is {arg}");                   // Noncompliant

        SerilogLog.Warning("Message without argument");                 // Compliant
        SerilogLog.Warning("The argument is {@Argument}", arg);         // Compliant
        SerilogLog.Warning($"The argument is {arg}");                   // Noncompliant
    }
}
