using System;
using NLog;

public class Program
{
    public void Log_Debug(ILogger logger)
    {
        logger.Debug("Debug 1");                                    // Noncompliant {{Reduce the number of Debug logging calls within this code block from 6 to the 4 allowed.}}
        logger.Debug("Debug 2: {Arg}", 42);                         // Secondary
        logger.Debug(new Exception(), "Debug 2");                   // Secondary
        logger.Trace("Debug 4");
        logger.Trace("Debug 5: {Arg}", 42);
        logger.Trace(new Exception(), "Debug 6");
        logger.ConditionalDebug("Debug 7");                         // Secondary
        logger.ConditionalDebug("Debug 8: {Arg}", 42);              // Secondary
        logger.ConditionalDebug(new Exception(), "Debug 9");        // Secondary
        logger.ConditionalTrace("Debug 10");
        logger.ConditionalTrace("Debug 11: {Arg}", 42);
        logger.ConditionalTrace(new Exception(), "Debug 12");

        while (true)
        {
            logger.Debug("Debug 1");                                // Compliant
            logger.Trace("Trace 1");
            logger.ConditionalDebug("Debug 2");
            logger.Debug("Debug 3");
            logger.ConditionalDebug("Debug 4");
            logger.ConditionalTrace("Trace 2");
        }
    }

    public static void Log_Trace(ILogger logger)
    {
        logger.Trace("Trace 1");                                    // Compliant
        logger.ConditionalTrace("Trace 2");
        logger.Log(LogLevel.Trace, "Trace 3");
        logger.Log(message: "Trace 4", level: LogLevel.Trace);
        logger.Trace("Trace 5: {Arg}", 42);
    }

    public static void Log_DebugAndTrace(ILogger logger)
    {
        logger.Trace("Trace 1");                                    // Compliant
        logger.Debug("Debug 1");                                    // Noncompliant {{Reduce the number of Debug logging calls within this code block from 5 to the 4 allowed.}}
        logger.ConditionalDebug("Debug 2");                         // Secondary
        logger.Log(LogLevel.Trace, "Trace 2");
        logger.Log(LogLevel.Debug, "Debug 3");                     // Secondary
        logger.Debug("Debug 4");                                    // Secondary
        logger.ConditionalDebug("Debug 5");                         // Secondary
    }

    public void Log_Error(ILogger logger)
    {
        logger.Error("Error 1");                                    // Noncompliant
        logger.Error("Error 2: {Arg}", 42);                         // Secondary
        logger.Fatal("Error 3");                                    // Secondary
        logger.Fatal("Error 4: {Arg}", 42);                         // Secondary

        while (true)
        {
            logger.Error("Error 1");                                // Compliant
        }
    }

    public void Log_Information(ILogger logger)
    {
        logger.Info("Info 1");                                      // Noncompliant
        logger.Info(new Exception(), "Info 2");                     // Secondary
        logger.Info("Info 3: {Arg}", 42);                           // Secondary
        logger.Info("Info 4: {Arg1} {Arg2}", 42, 42);               // Secondary

        while (true)
        {
            logger.Info("Info 1");                                  // Compliant
            logger.Info(new Exception(), "Info 2");
        }
    }

    public void Log_Warning(ILogger logger)
    {
        logger.Warn("Warn 1");                                      // Noncompliant
        logger.Warn(new Exception(), "Warn 2");                     // Secondary
        logger.Warn("Warn 3: {Arg}", 42);                           // Secondary
        logger.Warn("Warn 4: {Arg1} {Arg2}", 42, 42);               // Secondary

        while (true)
        {
            logger.Warn("Warn 1");                                  // Compliant
        }
    }
}
