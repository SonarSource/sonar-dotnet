using System;
using Microsoft.Extensions.Logging;

public class Program
{
    public void Log_Debug(ILogger logger)
    {
        logger.LogDebug("Debug 1");                                     // Compliant
        logger.LogDebug("Debug 2: {Arg}", 42);
        logger.LogDebug(new Exception(), "Debug 2");
        logger.LogTrace("Debug 4");
        logger.LogTrace("Debug 5: {Arg}", 42);
        logger.LogTrace(new Exception(), "Debug 6");
        logger.Log(LogLevel.Debug, "Debug 7");
        logger.Log(LogLevel.Trace, "Debug 8");

        while (true)
        {
            logger.LogDebug("Debug 1");                                 // Compliant
            logger.LogDebug("Debug 2: {Arg}", 42);
            logger.LogDebug(new Exception(), "Debug 3");
            logger.LogDebug("Debug 4");
            logger.LogTrace("Trace");
        }
    }

    public static void Log_Trace(ILogger logger)
    {
        logger.LogTrace("Trace 1");                                    // Compliant
        logger.Log(LogLevel.Trace, "Trace 2");
        logger.Log(message: "Trace 3", logLevel: LogLevel.Trace);
        LoggerExtensions.LogTrace(logger, "Trace 4");
        LoggerExtensions.Log(logger, LogLevel.Trace, "Trace 5");
    }

    public static void Log_DebugAndTrace(ILogger logger)
    {
        logger.LogTrace("Trace 1");                                    // Compliant
        logger.LogDebug("Debug 1");                                    // Noncompliant {{Reduce the number of Debug logging calls within this code block from 5 to the 4 allowed.}}
        logger.LogDebug("Debug 2");                                    // Secondary
        logger.Log(LogLevel.Trace, "Trace 2");
        logger.LogDebug("Debug 3");                                    // Secondary
        logger.LogDebug("Debug 4");                                    // Secondary
        logger.Log(LogLevel.Debug, "Debug 5");                         // Secondary
    }

    public void Log_Error(ILogger logger)
    {
        logger.LogError("Error 1");                                     // Noncompliant
        logger.LogError("Error 2: {Arg}", 42);                          // Secondary
        logger.LogCritical("Error 3");                                  // Secondary
        logger.LogCritical("Error 4: {Arg}", 42);                       // Secondary
        logger.Log(LogLevel.Error, "Error 5");                          // Secondary
        logger.Log(LogLevel.Critical, "Error 6");                       // Secondary

        while (true)
        {
            logger.LogError("Error 1");                                 // Compliant
        }
    }

    public void Log_Information(ILogger logger)
    {
        logger.LogInformation("Info 1");                                // Noncompliant
        logger.LogInformation(new Exception(), "Info 2");               // Secondary
        logger.LogInformation("Info 3: {Arg}", 42);                     // Secondary
        logger.Log(LogLevel.Information, "Info 4");                     // Secondary

        while (true)
        {
            logger.LogInformation("Info 1");                            // Compliant
            logger.LogInformation(new Exception(), "Info 2");
        }
    }

    public void Log_Warning(ILogger logger)
    {
        logger.LogWarning("Warn 1");                                    // Noncompliant
        logger.LogWarning(new Exception(), "Warn 2");                   // Secondary
        logger.LogWarning("Warn 3: {Arg}", 42);                         // Secondary
        logger.Log(LogLevel.Warning, "Warn 4");                         // Secondary

        while (true)
        {
            logger.LogWarning("Warn 1");                                // Compliant
        }
    }
}
