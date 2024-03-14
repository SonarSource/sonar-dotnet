using System;
using Serilog;
using Serilog.Events;
using static Serilog.Log;

public class Program
{
    public void Log_Debug()
    {
        Log.Debug("Debug 1");                                       // Noncompliant
        Log.Debug("Debug 2", new Exception());                      // Secondary
        Log.Verbose("Debug 3");                                     // Secondary
        Log.Verbose("Debug 5: {Arg1} {Arg2}", 42, 42);              // Secondary
        Log.Write(LogEventLevel.Debug, "Debug 6");                  // Secondary
        Log.Write(LogEventLevel.Verbose, "Debug 7");                // Secondary

        while (true)
        {
            Log.Debug("Debug 1");                                   // Compliant
            Log.Debug("Debug 2", new Exception());
            Log.Verbose("Debug 3");
            Log.Verbose("Debug 5: {Arg1} {Arg2}", 42, 42);
        }
    }

    public void Log_Error()
    {
        Log.Error("Error 1");                                       // Noncompliant
        Log.Error("Error 2: {Arg}", 42);                            // Secondary
        Log.Fatal("Error 3");                                       // Secondary
        Log.Fatal("Error 4: {Arg}", 42);                            // Secondary
        Log.Write(LogEventLevel.Error, "Error 6");                  // Secondary
        Log.Write(LogEventLevel.Fatal, "Error 7");                  // Secondary

        while (true)
        {
            Log.Error("Error 1");                                   // Compliant
        }
    }

    public void Log_Information()
    {
        Log.Information("Info 1");                                  // Noncompliant
        Log.Information("Info 2", new Exception());                 // Secondary
        Log.Information("Info 3: {Arg}", 42);                       // Secondary
        Log.Write(LogEventLevel.Information, "Info 4");             // Secondary

        while (true)
        {
            Log.Information("Info 1");                              // Compliant
            Log.Information("Info 2", new Exception());
        }
    }

    public void Log_Warning()
    {
        Log.Warning("Warn 1");                                      // Noncompliant
        Log.Warning("Warn 2", new Exception());                     // Secondary
        Log.Warning("Warn 3: {Arg}", 42);                           // Secondary
        Log.Write(LogEventLevel.Warning, "Info 4");                 // Secondary

        while (true)
        {
            Log.Warning("Warn 1");                                  // Compliant
        }
    }

    public void Log_ILogger(ILogger logger)
    {
        logger.Error("Error 1");                                    // Noncompliant
        logger.Error("Error 2: {Arg}", 42);                         // Secondary

        Log.Warning("Warn 1");                                      // Compliant
    }

    public void Log_UsingStatic(ILogger logger)
    {
        Error("Error 1");                                           // Noncompliant
        Error("Error 2: {Arg}", 42);                                // Secondary

        Warning("Warn 1");                                          // Compliant
    }
}
