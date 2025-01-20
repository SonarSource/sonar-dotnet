using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// Logging methods from: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions?view=dotnet-plat-ext-8.0
public class TestCases
{
    private readonly ILogger logger = new Logger<TestCases>(new NullLoggerFactory());

    public void MultipleLogsInTheSameCatch()
    {
        try { }
        catch (Exception e)
        {
            logger.LogWarning(new EventId(1), e, "Message!");
            logger.LogWarning(new EventId(1), e, "Message!");
        }

        try { }
        catch (DivideByZeroException e)
        {
            logger.LogWarning(new EventId(1), e, "Message!");
            logger.LogInformation(new EventId(1), "Message!");      // Compliant - the exception has been loged already
        }
        catch (Exception e)
        {
            logger.LogWarning(new EventId(1), e, "Message!");
            if (true)
            {
                logger.LogInformation(new EventId(1), "Message!");  // Compliant - the exception has been loged already
            }
        }

        try { }
        catch (Exception e)
        {
            logger.LogWarning(new EventId(1), "Message!");          // Noncompliant {{Logging in a catch clause should pass the caught exception as a parameter.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            logger.LogWarning(new EventId(1), "Message!");
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary {{Logging in a catch clause should pass the caught exception as a parameter.}}
        }
    }

    public void NoLogsInCatch()
    {
        try { }
        catch (Exception e)
        {
        }
    }

    public void CallingMethodToLog()
    {
        try { }
        catch (Exception e)
        {
            Log();
        }
    }

    private void Log()
    {
        logger.LogCritical("Message!"); // Compliant - we do not check this
    }

    public void LogFromLambda()
    {
        try { }
        catch (AggregateException e)
        {
            Call(() => logger.LogCritical("Message!"));             // FN - to avoid noise
        }
        catch (Exception e)
        {
            Call(() => logger.LogCritical(e, "Message!"));
        }

        try { }
        catch (Exception e)
        {
            Action<string> LogCritical = (string message) => { };
            LogCritical("Message");                                 // Compliant
        }
    }

    private void Call(Action action)
    {
        action();
    }

    private void LogFromMultipleCatchBlocks()
    {
        try { }
        catch (DivideByZeroException)
        {
            LoggerExtensions.LogCritical(logger, "Message!"); // Noncompliant
        }
        catch (AggregateException)
        {
            LoggerExtensions.LogCritical(logger, "Message!"); // Noncompliant
        }
        catch (ApplicationException e)
        {
            LoggerExtensions.LogCritical(logger, e, "Message!");
        }
    }

    private void LogFromNestedCatchBlocks(Exception wrongException)
    {
        try { }
        catch (Exception e)
        {
            logger.LogWarning("Message!");                      // Noncompliant
            logger.LogWarning(wrongException, "Message!");
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
            try { }
            catch (DivideByZeroException)
            {
                logger.LogCritical("Message!");                 // Noncompliant
            }
            catch (AggregateException e1)
            {
                logger.LogCritical(e, "Message!");              // Noncompliant - wrong exception
            }
            catch (ArgumentException e2)
            {
                logger.LogCritical(wrongException, "Message!"); // Compliant - caught exception is logged in the next line
                logger.LogCritical(e2, "Message!");
            }
            catch (Exception e3)
            {
                logger.LogCritical(e3, "Message!");
            }
        }
    }

    private void ReAssignment()
    {
        try { }
        catch (Exception e)
        {
            var other = e;
            logger.LogWarning(other, "Message!");     // Noncompliant - FP
        }
    }

    private void Filtering()
    {
        try { }
        catch (Exception e) when (e is InvalidCastException)
        {
            logger.LogWarning(e, "Message!");
        }
        catch (Exception e) when (e is DivideByZeroException divideByZeroException)
        {
            logger.LogWarning(divideByZeroException, "Message!");
        }
        catch (Exception e) when (e is InvalidOperationException invalidOperationException || e is InvalidTimeZoneException invalidTimeZoneException)
        {
            logger.LogWarning(e, "Message!");                                           // Compliant - the exception is logged (even if it has other names)
        }

        try { }
        catch (Exception e) when (e is InvalidCastException)
        {
            logger.LogWarning("Message!");                                              // Noncompliant
        }
        catch (Exception e) when (e is DivideByZeroException divideByZeroException)
        {
            logger.LogWarning(divideByZeroException, "Message!");                       // Compliant
        }
    }

    public void LogFromCatchBlockWithNoException()
    {
        try { }
        catch
        {
            logger.LogCritical("Message!");           // Noncompliant
        }
    }

    public void LogFromIfStatement()
    {
        try { }
        catch (DivideByZeroException e)
        {
            if (true)
            {
                logger.LogCritical("Message!");       // Noncompliant
            }
        }
        catch (Exception e)
        {
            if (true)
            {
                logger.LogCritical(e, "Message!");
            }
        }
    }

    public void LogFromSwitchStatement(bool condition)
    {
        try { }
        catch (DivideByZeroException e)
        {
            switch (condition)
            {
                case true:
                    logger.LogCritical("Message!");   // Noncompliant
                    break;
            }
        }
        catch (Exception e)
        {
            switch (condition)
            {
                case true:
                    logger.LogCritical(e, "Message!");
                    break;
            }
        }
    }

    public void LogFromCustomLogger()
    {
        try { }
        catch
        {
            new CustomLogger().LogCritical("Message!");
        }
    }

    public void LogOutsideCatchStatement()
    {
        logger.LogCritical("Message!");
    }

    public void ILoggerImplementation(NullLogger logger)
    {
        try { }
        catch (Exception e)
        {
            logger.LogCritical("Message!");     // Noncompliant
        }
        try { }
        catch (Exception e)
        {
            logger.LogCritical(e, "Message!");  // Compliant
        }
    }

    public void PartialLogging()
    {
        try { }
        catch (Exception e)
        {
            logger.LogWarning(new EventId(1), e.StackTrace, "Message!");        // Noncompliant
            logger.LogWarning(new EventId(1), (e.Message != null).ToString());
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
        }
    }

    public void LogInnerException()
    {
        try { }
        catch (Exception e)
        {
            logger.LogWarning(new EventId(1), e.InnerException, "Message!");                    // Compliant
            logger.LogWarning(new EventId(1), e?.InnerException, "Message!");
            logger.LogWarning(new EventId(1), e.InnerException.InnerException, "Message!");
            logger.LogWarning(new EventId(1), (Exception)e.InnerException, "Message!");
        }
    }

    public void LogFromCatchWithoutExceptionType()
    {
        try { }
        catch
        {
            logger.LogWarning("Message!");                                          // Noncompliant
        }
    }

    public class CustomLogger
    {
        public void LogCritical(string message) { }
    }
}
