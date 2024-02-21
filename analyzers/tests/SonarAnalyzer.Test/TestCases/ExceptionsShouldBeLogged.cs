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
            logger.LogWarning(new EventId(1), "Message!");          // Noncompliant
            logger.LogWarning(new EventId(1), "Message!");          // Noncompliant
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
            logger.LogWarning("Message!");                  // Noncompliant
            logger.LogWarning(wrongException, "Message!");  // Noncompliant - wrong exception
            try { }
            catch (DivideByZeroException)
            {
                logger.LogCritical("Message!");             // Noncompliant
            }
            catch (AggregateException e1)
            {
                logger.LogCritical(e, "Message!");          // Noncompliant - wrong exception
            }
            catch (Exception e2)
            {
                logger.LogCritical(e2, "Message!");
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
            logger.LogWarning(divideByZeroException, "Message!");   // Noncompliant - FP the exception has other name
        }
        catch (Exception e) when (e is InvalidOperationException invalidOperationException || e is InvalidTimeZoneException invalidTimeZoneException)
        {
            logger.LogWarning(e, "Message!");                       // Compliant - the exception is logged (even if it has other names)
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

    public class CustomLogger
    {
        public void LogCritical(string message) { }
    }
}
