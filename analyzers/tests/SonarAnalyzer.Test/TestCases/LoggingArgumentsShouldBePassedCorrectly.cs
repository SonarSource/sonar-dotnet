using System;
using Microsoft.Extensions.Logging;

public class Program
{
    private void Log(ILogger logger, Exception e)
    {
        LogWarning(null, null); // Error [CS0103] The name 'LogWarning' does not exist in the current context

        logger.LogCritical("Expected exception.", e);
//      ^^^^^^^^^^^^^^^^^^ {{Logging arguments should be passed to the correct parameter.}}
//                                                ^ Secondary @-1
        logger.LogWarning(new EventId(1), e.InnerException, "Message!");
        logger.LogWarning(new EventId(1), e?.InnerException, "Message!");
        logger.LogWarning(new EventId(1), e.InnerException.InnerException, "Message!");
        logger.LogWarning(new EventId(1), (Exception)e.InnerException, "Message!");
        logger.LogCritical(args: new object[] { e, LogLevel.Critical }, message: "Expected exception.");        // FN
        logger.LogCritical(message: "Expected exception.", eventId: new EventId(42), exception: new Exception(), args: e);
        logger.LogCritical(message: "Expected exception.", eventId: new EventId(42), args: e);
//      ^^^^^^^^^^^^^^^^^^
//                                                                                   ^^^^^^^ Secondary@-1
        logger.LogCritical("Expected exception.", new NullReferenceException());    // Noncompliant
                                                                                    // Secondary@-1
    }

    private void Log(CustomLogger logger, Exception e)
    {
        logger.LogCritical("Expected exception.", e, LogLevel.Critical);
//      ^^^^^^^^^^^^^^^^^^
//                                                ^ Secondary @-1
//                                                   ^^^^^^^^^^^^^^^^^ Secondary @-2
    }

    private class CustomLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        public bool IsEnabled(LogLevel logLevel) => false;
        public IDisposable BeginScope<TState>(TState state) => null;
    }
}
