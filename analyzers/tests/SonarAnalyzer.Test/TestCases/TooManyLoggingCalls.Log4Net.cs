using System;
using log4net;

public class Program
{
    public void Log_Debug(ILog logger)
    {
        logger.Debug("Debug 1");                                    // Noncompliant
        logger.Debug("Debug 2", new Exception());                   // Secondary
        logger.DebugFormat("Debug 4: {0}", 42);                     // Secondary
        logger.DebugFormat("Debug 5: {0} {1}", 42, 42);             // Secondary
        logger.Debug("Debug 5");                                    // Secondary

        while (true)
        {
            logger.Debug("Debug 1");                                // Compliant
            logger.Debug("Debug 2", new Exception());
            logger.DebugFormat("Debug 4: {0}", 42);
            logger.DebugFormat("Debug 5: {0} {1}", 42, 42);
        }
    }

    public void Log_Error(ILog logger)
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

    public void Log_Information(ILog logger)
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

    public void Log_Warning(ILog logger)
    {
        logger.Warn("Warn 1");                                      // Noncompliant
        logger.WarnFormat("Warn 2: {0}", 42);                       // Secondary


        while (true)
        {
            logger.Warn("Warn 1");                                  // Compliant
        }
    }
}
