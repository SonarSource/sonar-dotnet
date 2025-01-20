using System;
using Microsoft.Extensions.Logging;
using AliasedLogger = Microsoft.Extensions.Logging.ILogger;

public class Program
{
    private ILogger logger;

    public void Noncompliant_Basic()
    {
        logger.LogInformation("Info 1");    // Noncompliant {{Reduce the number of Information logging calls within this code block from 3 to the 2 allowed.}}
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        logger.LogInformation("Info 2");    // Secondary {{Reduce the number of Information logging calls within this code block from 3 to the 2 allowed.}}
        logger.LogInformation("Info 3");    // Secondary {{Reduce the number of Information logging calls within this code block from 3 to the 2 allowed.}}

        logger.LogError("Error 1");         // Noncompliant {{Reduce the number of Error logging calls within this code block from 2 to the 1 allowed.}}
        logger.LogError("Error 2");         // Secondary
    }

    public void Compliant_Basic()
    {
        logger.LogInformation("Info 1");    // Compliant - the number of calls in the same block doesn't exceed the treshold
        logger.LogInformation("Info 2");

        logger.LogDebug("Debug 1");
        logger.LogDebug("Debug 2");
        logger.LogDebug("Debug 3");
        logger.LogDebug("Debug 4");

        logger.LogError("Error 1");

        logger.LogWarning("Warning 1");
    }

    public void DifferentMethods_InSameLoggingCategory()
    {
        logger.LogDebug("Debug 1");         // Noncompliant
        logger.LogDebug("Debug 2");         // Secondary
        logger.LogTrace("Trace 1");         // Secondary
        logger.LogTrace("Trace 2");         // Secondary
        logger.LogTrace("Trace 3");         // Secondary

        logger.LogError("Error 1");         // Noncompliant
        logger.LogCritical("Critical 1");   // Secondary
    }

    public void MethodsInSameBlock_SeparatedByOtherBlocks_AreCountedTogether()
    {
        logger.LogInformation("Info 1");    // Noncompliant

        if (true)
        {
            Console.WriteLine();
        }

        logger.LogInformation("Info 2");    // Secondary

        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine();
        }

        logger.LogInformation("Info 3");    // Secondary
    }

    public void MethodsInDifferntBlocks_AreNotCountedTogether()
    {
        logger.LogInformation("Info 1");    // Compliant - the logging method count could exceed the treshold if they were in the same block, but they aren't

        if (true)
        {
            logger.LogInformation("Info 2");
        }

        for (int i = 0; i < 10; i++)
        {
            logger.LogInformation("Info 3");

            for (int j = 0; j < 10; j++)
            {
                logger.LogInformation("Info 4");
            }
        }
    }

    public void NestedBlocks()
    {
        if (true)
        {
            logger.LogInformation("Info 1");    // Noncompliant
            logger.LogInformation("Info 2");    // Secondary
            logger.LogInformation("Info 3");    // Secondary
        }

        for (int i = 0; i < 10; i++)
        {
            logger.LogError("Error 1");         // Noncompliant

            for (int j = 0; j < 10; j++)
            {
                logger.LogWarning("Warning 1"); // Noncompliant
                logger.LogWarning("Warning 2"); // Secondary
            }

            logger.LogError("Error 2");         // Secondary
        }
    }

    public void MethodOverloads(int arg1, int arg2)
    {
        logger.LogDebug("Debug 1");                                                     // Noncompliant
        logger.LogDebug("Debug 2: {Arg1}", arg1);                                       // Secondary
        logger.LogDebug("Debug 3: {Arg1} {Arg2}", arg1, arg2);                          // Secondary
        logger.LogDebug(new EventId(42), "Debug 4: {Arg1}", arg1);                      // Secondary
        logger.LogDebug(new EventId(42), new Exception(), "Debug 5: {Arg1}", arg1);     // Secondary

        logger.LogError("Error 1");                                                     // Noncompliant
        logger.LogError(new EventId(42), new Exception(), "Error 2: {Arg1}", arg1);     // Secondary
    }

    public void LogMethod_WithLogLevelParameter()
    {
        logger.Log(LogLevel.Debug, "Debug 1");                      // Noncompliant
        logger.Log(message: "Debug 2", logLevel: LogLevel.Trace);   // Secondary
        logger.LogDebug("Debug 3");                                 // Secondary
        logger.LogTrace("Debug 4");                                 // Secondary
        logger.Log(LogLevel.Debug, "Debug 5");                      // Secondary

        logger.Log(LogLevel.Error, "Error 1");                      // Noncompliant
        logger.LogError("Error 2");                                 // Secondary

        logger.Log(LogLevel.None, "None 1");                        // Compliant - LogLevel.None doesn't fall to any of the tracked logging categories
        logger.Log(LogLevel.None, "None 2");
        logger.Log(LogLevel.None, "None 3");
        logger.Log(LogLevel.None, "None 4");
        logger.Log(LogLevel.None, "None 5");
    }

    public void LocalMethodsAndLambdas()
    {
        Action<int> simpleLambda = arg =>
        {
            logger.LogError("Error 1");                             // Noncompliant
            logger.LogError("Error 2");                             // Secondary
        };

        Action parenthesizedLambda = () =>
        {
            logger.LogError("Error 1");                             // Noncompliant
            logger.LogError("Error 2");                             // Secondary
        };

        Action methodDelegate = delegate()
        {
            logger.LogError("Error 1");                             // Noncompliant
            logger.LogError("Error 2");                             // Secondary
        };

        Action a1 = () => logger.LogInformation("Information 1");   // Compliant - the log methods are in different lambdas
        Action a2 = () => logger.LogInformation("Information 2");
        Action a3 = () => logger.LogInformation("Information 3");

        Action<string> LogWarning = message => Console.WriteLine("Warning: " + message);
        LogWarning("Warning 1");                                    // Compliant - the method is not from known logging frameworks
        LogWarning("Warning 2");

        void LocalFunction()
        {
            logger.LogError("Error 1");                             // Noncompliant
            logger.LogError("Error 2");                             // Secondary
        }
    }

    public void IfStatements(int arg)
    {
        if (arg == 0)
        {
            logger.LogError("Error 1");         // Noncompliant
            logger.LogError("Error 2");         // Secondary
        }
        else if (arg == 1)
        {
            logger.LogError("Error 1");         // Noncompliant
            logger.LogError("Error 2");         // Secondary
        }
        else
        {
            logger.LogError("Error 1");         // Noncompliant
            logger.LogError("Error 2");         // Secondary
        }

        if (arg == 0)
            logger.LogError("Error 1");         // Compliant - only one logging statement in each branch
        else if (arg == 1)
            logger.LogError("Error 2");
        else
            logger.LogError("Error 2");
    }


    public void SwitchStatements(int arg)
    {
        switch (arg)
        {
            case 0:
                logger.LogError("Error 1");             // Compliant - the logging methods are in different branches
                break;
            default:
                logger.LogError("Error 2");
                break;
        }

        switch (arg)
        {
            case 0:
                logger.LogWarning("Warning 1");         // FN
                logger.LogWarning("Warning 2");
                break;
            case 1:
                break;
        }
    }

    public void UsingAliasedLogger(AliasedLogger aliasedLogger)
    {
        aliasedLogger.LogError("Error 1");              // Noncompliant
        aliasedLogger.LogError("Error 2");              // Secondary
    }

    public void LoggerExtensionMethods()
    {
        LoggerExtensions.LogError(logger, "Error 1");   // Noncompliant
        logger.LogError("Error 2");                     // Secondary
    }

    public int Property
    {
        get
        {
            logger.LogError("Error 1");                 // Noncompliant
            logger.LogError("Error 2");                 // Secondary
            return 42;
        }
        set
        {
            logger.LogError("Error 1");                 // Noncompliant
            logger.LogError("Error 2");                 // Secondary
        }
    }

    public event EventHandler MyEvent
    {
        add
        {
            logger.LogError("Error 1");                 // Noncompliant
            logger.LogError("Error 2");                 // Secondary
        }
        remove
        {
            logger.LogError("Error 1");                 // Noncompliant
            logger.LogError("Error 2");                 // Secondary
        }
    }
}

namespace MyNamespace
{
    public interface ILogger { }
    public static class FakeLoggerExtensions
    {
        public static void LogError(this ILogger logger, string message) { }
    }

    public class Test
    {
        public void FakeLoggerMethods(ILogger logger)
        {
            FakeLoggerExtensions.LogError(logger, "Error 1");   // Compliant - not a supported logger type
            logger.LogError("Error 2");
        }
    }
}
