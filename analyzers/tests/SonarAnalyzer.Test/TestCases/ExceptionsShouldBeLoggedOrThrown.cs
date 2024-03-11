using System;
using Microsoft.Extensions.Logging;

public class ExceptionsShouldBeEitherLoggedOrThrown
{
    public void Generic(ILogger<ExceptionsShouldBeEitherLoggedOrThrown> logger)
    {
        try
        {
        }
        catch (DivideByZeroException divideByZeroException)             // The exception is only logged.
        {
            logger.LogError(divideByZeroException, "Message!");
        }
        catch (OverflowException exception)                             // The exception is only thrown
        {
            throw;
        }
        catch (FormatException exception)                               // The exception is only thrown
        {
            throw exception;
        }
        catch (ArithmeticException exception)                           // The exception is not logged
        {
            logger.LogError(null, "Message!");
            throw;
        }
        catch (RankException exception)                                 // Logging a different exception
        {
            logger.LogError(new Exception("hello"), "Message!");
            throw;
        }
        catch (AggregateException exception)                            // Logging a different exception
        {
            logger.LogError(exception, "Message!");
            throw new Exception();
        }
        catch (InvalidOperationException loggedMultipleTimes)           // Noncompliant {{Either log this exception and handle it, or rethrow it with some contextual information.}}
        {
            logger.LogError(loggedMultipleTimes, "Message!");
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary {{Logging statement.}}
            logger.LogInformation(loggedMultipleTimes, "Message!");
            throw;
//          ^^^^^^ Secondary {{Thrown exception.}}
        }
        catch (ArgumentException misplaced)                             // Noncompliant
        {
            logger.LogError("Message!", misplaced);                     // Secondary
            throw;                                                      // Secondary
        }
        catch (InvalidCastException invalidCastException)               // Noncompliant
        {
            logger.LogError(invalidCastException, "Message!");          // Secondary
            throw invalidCastException;                                 // Secondary
        }
        catch (Exception ex)                                            // Noncompliant
        {
            logger.LogError(ex, "Message!");                            // Secondary
            throw;                                                      // Secondary
        }
    }

    public void NonGeneric(ILogger logger)
    {
        try {}
        catch (Exception ex)                                            // Noncompliant
        {
            logger.LogError(ex, "Message!");                            // Secondary
            throw;                                                      // Secondary
        }
    }

    public void Nested(ILogger logger)
    {
        try {}
        catch (Exception outer)                                         // FN - Only the main catch body is visited
        {
            try {}
            catch (Exception inner)
            {
                logger.LogError(outer, "Message");
            }

            try {}
            catch (Exception inner)                                     // Noncompliant
            {
                logger.LogError(inner, "Message");                      // Secondary
                throw;                                                  // Secondary
            }

            throw;
        }

        try {}
        catch (Exception outer)
        {
            logger.LogError(outer, "Message!");
            try {}
            catch (Exception inner)
            {
                throw;
            }
        }

        try {}
        catch (Exception outer)                                         // FN - Only the main catch body is visited
        {
            logger.LogError(outer, "Message!");
            try {}
            catch (Exception inner)
            {
                throw outer;
            }
        }

        try { }
        catch (Exception outer)                                         // Noncompliant
        {
            try { }
            finally
            {
                logger.LogError(outer, "message");                      // Secondary
                throw outer;                                            // Secondary
            }
        }
    }

    public void NoCatch(ILogger logger, Exception exception)
    {
        logger.LogError(exception, "Message!");                         // Compliant - there is not catch block
        throw exception;
    }

    public void Filtering(ILogger logger)
    {
        try {}
        catch (Exception ex) when (ex is DivideByZeroException d)       // Noncompliant
        {
            logger.LogError(ex, "Message!");                            // Secondary
            throw;                                                      // Secondary
        }

        try {}
        catch (Exception ex) when (ex is DivideByZeroException d)       // FN
        {
            logger.LogError(ex, "Message!");
            throw d;
        }
    }

    private void With(CustomLogger logger)
    {
        try { }
        catch (Exception exception)                                     // The exception is only logged.
        {
            logger.LogError(exception, "Message!");
        }
    }

    public void LoggingFromAnotherMethod(ILogger logger)
    {
        try { }
        catch (Exception e)                                             // FN
        {
            Log(logger, e);
            throw;
        }
    }

    private void Log(ILogger logger, Exception e)
    {
        logger.LogCritical("Message!");                                 // Compliant - we do not check this
        throw e;
    }

    public void LogFromLambda(ILogger logger)
    {
        try { }
        catch (Exception e)                                             // FN - to avoid noise in other lambda cases
        {
            Call(() => logger.LogCritical(e, "Message!"));
            throw;
        }

        try { }
        catch (Exception e)
        {
            Action<Exception, string> LogCritical = (Exception ex, string message) => { };
            LogCritical(e, "Message");
            throw;
        }

        try
        { }
        catch (Exception exception)
        {
            Action x = () => { throw exception; };
            logger.LogError(exception, "Message");
        }

        try
        { }
        catch (Exception exception)
        {
            Action x = () => { logger.LogError(exception, "Message"); };
            throw exception;
        }
    }

    private void ReAssignment(ILogger logger)
    {
        try { }
        catch (InvalidOperationException e)                             // FN
        {
            var other = e;
            logger.LogWarning(other, "Message!");
            throw;
        }
        catch (Exception e)                                             // FN
        {
            var other = e;
            logger.LogWarning(other, "Message!");
            throw e;
        }
    }

    public void LogPartial_Compliant(ILogger logger)
    {
        try { }
        catch (Exception e)
        {
            logger.LogWarning("Message!" + e.Message);
            logger.LogWarning(e.Message);
            logger.LogWarning(e?.InnerException, "Message!");
            logger.LogWarning(e.InnerException.InnerException, "Message!");
            logger.LogWarning((Exception)e.InnerException, "Message!");

            throw;
        }
    }
    public void LogPartial_Notcompliant(ILogger logger)
    {
        try { }
        catch (InvalidCastException e)                                  // Noncompliant
        {
            logger.LogWarning(e.InnerException, "Message!");            // Secondary
            throw;                                                      // Secondary
        }
    }

    public void MultipleThrows(ILogger logger)
    {
        try { }
        catch (InvalidCastException e)                                  // Noncompliant
        {
            logger.LogWarning(e.InnerException, "Message!");            // Secondary
            throw new Exception();
            throw;                                                      // Secondary
        }

        try { }
        catch (InvalidCastException e)                                  // Noncompliant
        {
            logger.LogWarning(e.InnerException, "Message!");            // Secondary
            throw new Exception();
            throw e;                                                      // Secondary
        }

        try { }
        catch (InvalidCastException e)                                  // Noncompliant
        {
            logger.LogWarning(e.InnerException, "Message!");            // Secondary
            throw;                                                      // Secondary
            throw new Exception();
        }
    }

    public void LogFromCatchWithoutException(ILogger logger)
    {
        try { }
        catch
        {
            logger.LogWarning("Message! Stack trace: {StackTrace}", new System.Diagnostics.StackTrace());
            throw;
        }
    }

    public void Branches(ILogger logger, bool condition, object x)
    {
        try { }
        catch (InvalidOperationException e)                     // FN
        {
            if (condition)
            {
                logger.LogError(e, "Message!");
                throw;
            }
        }
        catch (ArgumentException e)                             // Compliant - conditional
        {
            logger.LogError(e, "Message!");
            if (condition)
            {
                throw;
            }
        }
        catch (AggregateException e)                            // Compliant - conditional
        {
            if (condition)
            {
                logger.LogError(e, "Message!");
            }
            throw;
        }
        catch (ApplicationException e)                          // Compliant - conditional
        {
            if (condition)
            {
                logger.LogError(e, "Message!");
            }
            else
            {
                throw;
            }
        }
        catch (FormatException e)                               // Noncompliant
        {
            if (condition)
            {
            }
            logger.LogError(e, "Message!");                     // Secondary
            throw;                                              // Secondary
        }
        catch (OverflowException e)                             // Compliant - conditional
        {
            switch (condition)
            {
                case true:
                    logger.LogError(e, "Message!");
                    break;
                case false:
                    throw;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Message!");
            x = condition ? 42 : throw e;
        }
    }

    private void Call(Action action)
    {
        action();
    }

    private class CustomLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => null;
    }
}
