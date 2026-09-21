using System;
using System.Collections.Generic;
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

    private void RethrowFromOuterCatch()
    {
        try { }
        catch (Exception outer)
        {
            logger.LogWarning("Outer exception"); // Compliant - rethrown below
            try { }
            catch (Exception inner)
            {
                logger.LogWarning("Inner exception"); // Noncompliant
            }
            throw;
        }
    }

    public void Rethrow_BareThrow(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            throw;
        }
    }

    public void Rethrow_CaughtException(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            throw e;
        }
    }

    public void Rethrow_ParenthesizedException(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            throw (e);
        }
    }

    public void Rethrow_LocalAlias(string message)
    {
        try { }
        catch (Exception e)
        {
            var ex = e;
            logger.LogError(message); // Noncompliant - FP: local aliases of the caught exception are not tracked.
            throw ex;
        }
    }

    public void Rethrow_FromUsing(string message, Func<IDisposable> open)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            using (var resource = open())
            {
                throw;
            }
        }
    }

    public void Rethrow_FromLock(string message, object gate)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            lock (gate)
            {
                throw e;
            }
        }
    }

    public void Rethrow_FromTryFinally(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            try
            {
                throw e;
            }
            finally { }
        }
    }

    public void Rethrow_FromFinally(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            try { }
            finally
            {
                throw e;
            }
        }
    }

    public void Rethrow_FromFinallyAfterCatch(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            try { }
            catch { }
            finally
            {
                throw e;
            }
        }
    }

    public void Rethrow_FromFinallyAfterReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            try
            {
                if (condition)
                {
                    return;
                }
            }
            finally
            {
                throw e;
            }
        }
    }

    public void Rethrow_FromFinallyAfterReturnWithCatch(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            try
            {
                if (condition)
                {
                    return;
                }
            }
            catch { }
            finally
            {
                throw e;
            }
        }
    }

    public void Rethrow_FromFinallyAfterGoto(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            try
            {
                if (condition)
                {
                    goto End;
                }
            }
            finally
            {
                throw e;
            }
            End: return;
        }
    }

    public void Rethrow_FromFinallyAfterBreak(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            do
            {
                try
                {
                    if (condition)
                    {
                        break;
                    }
                }
                finally
                {
                    throw e;
                }
            } while (condition);
        }
    }

    public void Rethrow_FromFinallyAfterContinue(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            do
            {
                try
                {
                    if (condition)
                    {
                        continue;
                    }
                }
                finally
                {
                    throw e;
                }
            } while (condition);
        }
    }

    public void Rethrow_FromOuterFinallyAfterReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            try
            {
                if (condition)
                {
                    return;
                }
                try { }
                finally
                {
                    throw e;
                }
            }
            finally
            {
                throw e;
            }
        }
    }

    public void Rethrow_FromDoWhile(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            do // The body of a do-while always runs at least once.
            {
                throw e;
            } while (condition);
        }
    }

    public void Rethrow_AfterWhileBreak(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            while (condition)
            {
                if (condition)
                {
                    break;
                }
            }
            throw;
        }
    }

    public void Rethrow_AfterWhileContinue(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            while (condition)
            {
                if (condition)
                {
                    continue;
                }
            }
            throw;
        }
    }

    public void Rethrow_AfterSwitchBreak(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            switch (condition)
            {
                case true:
                    break;
            }
            throw;
        }
    }

    public void Rethrow_AfterDoWhileBreak(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            do
            {
                if (condition)
                {
                    break;
                }
            } while (condition);
            throw;
        }
    }

    public void Rethrow_AfterAnonymousMethodReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            Action action = delegate
            {
                if (condition)
                {
                    return;
                }
            };
            throw;
        }
    }

    public void Rethrow_AfterLocalFunctionReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            void Local()
            {
                if (condition)
                {
                    return;
                }
            }
            throw;
        }
    }

    public void Rethrow_AtLabel(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            if (condition)
            {
                goto Rethrow;
            }
            Rethrow:
            throw;
        }
    }

    public void Rethrow_AfterEmptyLabel(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            if (condition)
            {
                goto Rethrow;
            }
            Rethrow:
            { }
            throw;
        }
    }

    public void Rethrow_AfterBackwardGoto(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            Again:
            if (condition)
            {
                goto Again;
            }
            throw;
        }
    }

    public void Rethrow_AfterBackwardGotoToBlock(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            Again:
            {
                if (condition)
                {
                    goto Again;
                }
            }
            throw e;
        }
    }

    public void Rethrow_AfterGotoCase(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            switch (condition)
            {
                case true:
                    goto case false;
                case false:
                    break;
            }
            throw;
        }
    }

    public void Rethrow_AfterGotoDefault(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            switch (condition)
            {
                case true:
                    goto default;
                default:
                    break;
            }
            throw;
        }
    }

    public void Rethrow_AfterLocalFunctionGoto(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Compliant - the caught exception is rethrown
            void Local()
            {
                Again:
                if (condition)
                {
                    goto Again;
                }
            }
            throw;
        }
    }

    // A conditional rethrow leaves the paths that do not throw without the exception context, so the logging call is still reported.
    // S2139 cannot cover these: it skips conditionals on purpose and requires the exception to be passed to the logger.
    public void ConditionalRethrow_If(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            if (condition)
            {
                throw;
            }
        }
    }

    public void ConditionalRethrow_Switch(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            switch (condition)
            {
                case true:
                    throw e;
            }
        }
    }

    public void ConditionalRethrow_ConditionalExpression(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            var value = condition ? message : throw e;
        }
    }

    public void ConditionalRethrow_CoalesceExpression(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            var value = message ?? throw e;
        }
    }

    public void ConditionalRethrow_While(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            while (condition)
            {
                throw e;
            }
        }
    }

    public void ConditionalRethrow_For(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            for (var i = 0; condition; i++)
            {
                throw e;
            }
        }
    }

    public void ConditionalRethrow_ForEach(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            foreach (var c in message)
            {
                throw e;
            }
        }
    }

    public void ConditionalRethrow_ForEachDeconstruction(string message, IEnumerable<(int, int)> pairs)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            foreach (var (a, b) in pairs)
            {
                throw e;
            }
        }
    }

    public void ConditionalRethrow_AfterReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            if (condition)
            {
                return;
            }
            throw;
        }
    }

    public void ConditionalRethrow_AfterWhileReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            while (condition)
            {
                return;
            }
            throw e;
        }
    }

    public void ConditionalRethrow_AfterGoto(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            if (condition)
            {
                goto End;
            }
            throw;
            End: return;
        }
    }

    public void ConditionalRethrow_AfterBackwardGotoWithReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            Again:
            if (condition)
            {
                return;
            }
            if (condition)
            {
                goto Again;
            }
            throw;
        }
    }

    public void ConditionalRethrow_AfterBackwardGotoWithExit(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            Again:
            if (condition)
            {
                goto End;
            }
            if (condition)
            {
                goto Again;
            }
            throw;
            End: return;
        }
    }

    public void ConditionalRethrow_FinallyAfterReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            if (condition)
            {
                return;
            }
            try { }
            finally
            {
                throw e;
            }
        }
    }

    public void ConditionalRethrow_FinallyAfterOuterAndInnerReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            if (condition)
            {
                return;
            }
            try
            {
                if (condition)
                {
                    return;
                }
            }
            finally
            {
                throw e;
            }
        }
    }

    public void ConditionalRethrow_AfterTryFinallyWithReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            try
            {
                if (condition)
                {
                    return;
                }
            }
            finally { }
            throw;
        }
    }

    public void ConditionalRethrow_NestedFinallyAfterReturn(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            try
            {
                if (condition)
                {
                    return;
                }
                try { }
                finally
                {
                    throw e;
                }
            }
            finally { }
        }
    }

    public void ConditionalRethrow_FinallyInsideIf(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            if (condition)
            {
                try
                {
                    return;
                }
                finally
                {
                    throw e;
                }
            }
        }
    }

    public void ConditionalRethrow_ConditionalFinally(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            try
            {
                if (condition)
                {
                    return;
                }
            }
            finally
            {
                if (condition)
                {
                    throw e;
                }
            }
        }
    }

    public void ConditionalRethrow_DoWhileBreak(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            do
            {
                if (condition)
                {
                    break;
                }
                throw;
            } while (condition);
        }
    }

    public void ConditionalRethrow_DoWhileContinue(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            do
            {
                if (condition)
                {
                    continue;
                }
                throw e;
            } while (condition);
        }
    }

    public void ConditionalRethrow_DoWhileSwitchContinue(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            do
            {
                switch (condition)
                {
                    case true:
                        continue;
                }
                throw;
            } while (condition);
        }
    }

    public void ConditionalRethrow_CaughtBareThrow(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            try
            {
                throw;
            }
            catch { }
        }
    }

    public void ConditionalRethrow_CaughtExceptionThrow(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            try
            {
                throw e;
            }
            catch { }
        }
    }

    public void JumpOutOfCatch_Break(string message, bool condition)
    {
        while (condition)
        {
            try { }
            catch (Exception e)
            {
                logger.LogError(message); // Noncompliant
                if (condition)
                {
                    break;
                }
                throw;
            }
        }
    }

    public void JumpOutOfCatch_Continue(string message, bool condition)
    {
        while (condition)
        {
            try { }
            catch (Exception e)
            {
                logger.LogError(message); // Noncompliant
                if (condition)
                {
                    continue;
                }
                throw;
            }
        }
    }

    public void InvalidJump_Break(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message);
            break; // Error [CS0139]
            throw;
        }
    }

    public void InvalidJump_Continue(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message);
            continue; // Error [CS0139]
            throw;
        }
    }

    public void GotoOutOfCatch_End(string message, bool condition)
    {
        Before: ;
        Retry:
        switch (condition)
        {
            case true:
                try { }
                catch (Exception e)
                {
                    logger.LogError(message); // Noncompliant
                    if (condition)
                    {
                        goto End;
                    }
                    throw;
                }
                break;
            case false:
            default:
                break;
        }
        End: return;
    }

    public void GotoOutOfCatch_Before(string message, bool condition)
    {
        Before: ;
        Retry:
        switch (condition)
        {
            case true:
                try { }
                catch (Exception e)
                {
                    logger.LogError(message); // Noncompliant
                    if (condition)
                    {
                        goto Before;
                    }
                    throw;
                }
                break;
            case false:
            default:
                break;
        }
        End: return;
    }

    public void GotoOutOfCatch_Retry(string message, bool condition)
    {
        Before: ;
        Retry:
        switch (condition)
        {
            case true:
                try { }
                catch (Exception e)
                {
                    logger.LogError(message); // Noncompliant
                    if (condition)
                    {
                        goto Retry;
                    }
                    throw;
                }
                break;
            case false:
            default:
                break;
        }
        End: return;
    }

    public void GotoOutOfCatch_Case(string message, bool condition)
    {
        Before: ;
        Retry:
        switch (condition)
        {
            case true:
                try { }
                catch (Exception e)
                {
                    logger.LogError(message); // Noncompliant
                    if (condition)
                    {
                        goto case false;
                    }
                    throw;
                }
                break;
            case false:
            default:
                break;
        }
        End: return;
    }

    public void GotoOutOfCatch_Default(string message, bool condition)
    {
        Before: ;
        Retry:
        switch (condition)
        {
            case true:
                try { }
                catch (Exception e)
                {
                    logger.LogError(message); // Noncompliant
                    if (condition)
                    {
                        goto default;
                    }
                    throw;
                }
                break;
            case false:
            default:
                break;
        }
        End: return;
    }

    // A "when" filter declaration pattern binds an alias of the caught exception, so rethrowing it rethrows the caught exception.
    public void WhenFilterAliasRethrow(string message)
    {
        try { }
        catch (Exception e) when (e is InvalidOperationException alias)
        {
            logger.LogError(message); // Compliant - alias is the caught exception and it is rethrown
            throw alias;
        }
    }

    public void WhenFilterAliasRethrow_VarPattern(string message)
    {
        try { }
        catch (Exception e) when (e is var alias)
        {
            logger.LogError(message); // Noncompliant - FP: var pattern aliases of the caught exception are not tracked.
            throw alias;
        }
    }

    // Only a pattern matched directly against the caught exception is an alias.
    public void WhenFilterNotAnAlias_InnerException(string message)
    {
        try { }
        catch (Exception e) when (e.InnerException is Exception other)
        {
            logger.LogError(message); // Noncompliant
            throw other;
        }
    }

    public void WhenFilterNotAnAlias_DifferentException(string message)
    {
        try { }
        catch (Exception e) when (e is InvalidOperationException alias)
        {
            logger.LogError(message); // Noncompliant
            throw new Exception();
        }
    }

    // Anonymous methods and local functions are visited: a throw inside them is not a rethrow, but their logging calls are still reported.
    public void LoggingInsideNestedFunction_AnonymousMethod(string message)
    {
        try { }
        catch (Exception e)
        {
            Action action = delegate
            {
                logger.LogError(message); // Noncompliant
            };
        }
    }

    public void LoggingInsideNestedFunction_LocalFunction(string message)
    {
        try { }
        catch (Exception e)
        {
            void Log()
            {
                logger.LogError(message); // Noncompliant
            }
        }
    }

    public void LoggingInsideNestedFunction_ExpressionBodiedLocalFunction(string message)
    {
        try { }
        catch (Exception e)
        {
            void Log() => logger.LogError(message); // Noncompliant
        }
    }

    public void UnrelatedThrow_NewException(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            throw new Exception();
        }
    }

    public void UnrelatedThrow_OtherException(string message, Exception other)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            throw other;
        }
    }

    public void UnrelatedThrow_InnerException(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant - throwing the inner exception does not rethrow the caught exception.
            throw e.InnerException;
        }
    }

    public void UnrelatedThrow_CoalesceExpression(string message, Exception other)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            var value = message ?? throw other;
        }
    }

    public void UnrelatedThrow_NestedBareThrow(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            try { }
            catch
            {
                throw;
            }
        }
    }

    public void UnrelatedThrow_NestedException(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            try { }
            catch (Exception nested)
            {
                throw nested;
            }
        }
    }

    public void UnrelatedThrow_Lambda(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            Action action = () =>
            {
                throw e;
            };
        }
    }

    public void UnrelatedThrow_LambdaWithParameter(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            Action<bool> action = _ =>
            {
                throw e;
            };
        }
    }

    public void UnrelatedThrow_AnonymousMethod(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            Action action = delegate
            {
                throw e;
            };
        }
    }

    public void UnrelatedThrow_LocalFunction(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            void Rethrow()
            {
                throw e;
            }
        }
    }

    public void UnrelatedThrow_ExpressionBodiedLocalFunction(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            void Rethrow() => throw e;
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
