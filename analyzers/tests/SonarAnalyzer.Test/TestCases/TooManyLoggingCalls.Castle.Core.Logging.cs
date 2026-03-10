using System;
using Castle.Core.Logging;

public class Program
{
    public void Log_Debug(ILogger logger)
    {
        logger.Debug("Debug 1");                                    // Noncompliant
        logger.Debug("Debug 2", new Exception());                   // Secondary
        logger.DebugFormat("Debug 3: {0}", 42);                     // Secondary
        logger.DebugFormat("Debug 4: {0} {1}", 42, 42);             // Secondary
        logger.Debug("Debug 5");                                    // Secondary

        while (true)
        {
            logger.Debug("Debug 1");                                // Compliant
            logger.Debug("Debug 2", new Exception());
            logger.DebugFormat("Debug 3: {0}", 42);
            logger.DebugFormat("Debug 4: {0} {1}", 42, 42);
        }
    }

    public void Log_Trace(ILogger logger)
    {
        logger.Trace("Trace 1");                                    // Noncompliant - FP NET-3295
        logger.Trace("Trace 2", new Exception());                   // Secondary - FP NET-3295
        logger.TraceFormat("Trace 3: {0}", 42);                     // Secondary - FP NET-3295
        logger.TraceFormat("Trace 4: {0} {1}", 42, 42);             // Secondary - FP NET-3295
        logger.Trace("Trace 5");                                    // Secondary - FP NET-3295

        while (true)
        {
            logger.Trace("Trace 1");                                // Compliant
            logger.Trace("Trace 2", new Exception());
            logger.TraceFormat("Trace 3: {0}", 42);
            logger.TraceFormat("Trace 4: {0} {1}", 42, 42);
        }
    }

    public void Log_Error(ILogger logger)
    {
        logger.Error("Error 1");                                    // Noncompliant
        logger.ErrorFormat("Error 2: {0}", 42);                     // Secondary
        logger.Fatal("Error 3");                                    // Secondary
        logger.FatalFormat("Error 4: {0}", 42);                     // Secondary

        while (true)
        {
            logger.Error("Error 1");                                // Compliant
        }
    }

    public void Log_Information(ILogger logger)
    {
        logger.Info("Info 1");                                      // Noncompliant
        logger.Info("Info 2", new Exception());                     // Secondary
        logger.InfoFormat("Info 3: {0}", 42);                       // Secondary

        while (true)
        {
            logger.Info("Info 1");                                  // Compliant
            logger.Info("Info 2", new Exception());
        }
    }

    public void Log_Warning(ILogger logger)
    {
        logger.Warn("Warn 1");                                      // Noncompliant
        logger.WarnFormat("Warn 2: {0}", 42);                       // Secondary


        while (true)
        {
            logger.Warn("Warn 1");                                  // Compliant
        }
    }
}
