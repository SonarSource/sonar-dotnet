using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class Program
{
    private void Log(ILogger logger, Exception e)
    {
        logger.LogCritical("Expected exception.", e);       // Noncompliant {{Logging arguments should be passed to the correct parameter.}}
//                                                ^
        logger.LogWarning(new EventId(1), e.InnerException, "Message!");                    // Compliant
        logger.LogWarning(new EventId(1), e?.InnerException, "Message!");
        logger.LogWarning(new EventId(1), e.InnerException.InnerException, "Message!");
        logger.LogWarning(new EventId(1), (Exception)e.InnerException, "Message!");
    }

    private void Log(CustomLogger logger, Exception e)
    {
        logger.LogCritical("Expected exception.", e);       // Noncompliant
    }

    private class CustomLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        public bool IsEnabled(LogLevel logLevel) => false;
        public IDisposable BeginScope<TState>(TState state) => null;
    }
}
