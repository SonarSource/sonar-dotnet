using System;
using NLog;

public class Program
{
    public void Log_Debug(ILogger logger)
    {
        logger.Debug("Debug 1");                                    // Noncompliant
        logger.Debug("Debug 2: {Arg}", 42);                         // Secondary
        logger.Debug(new Exception(), "Debug 2");                   // Secondary
        logger.Trace("Debug 4");                                    // Secondary
        logger.Trace("Debug 5: {Arg}", 42);                         // Secondary
        logger.Trace(new Exception(), "Debug 6");                   // Secondary
        logger.ConditionalDebug("Debug 7");                         // Secondary
        logger.ConditionalDebug("Debug 8: {Arg}", 42);              // Secondary
        logger.ConditionalDebug(new Exception(), "Debug 9");        // Secondary
        logger.ConditionalTrace("Debug 10");                        // Secondary
        logger.ConditionalTrace("Debug 11: {Arg}", 42);             // Secondary
        logger.ConditionalTrace(new Exception(), "Debug 12");       // Secondary

        while (true)
        {
            logger.Debug("Debug 1");                                // Compliant
            logger.Trace("Debug 2");
            logger.ConditionalDebug("Debug 3");
            logger.ConditionalTrace("Debug 4");
        }
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
