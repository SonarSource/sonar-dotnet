using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class CSharp8TestCases
{
    private readonly ILogger logger = new Logger<CSharp8TestCases>(new NullLoggerFactory());

    public void ConditionalRethrow_InvalidCoalesceAssignment(string message)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            message ??= throw e; // Error [CS8115] - Throw expressions are not allowed directly in coalescing assignments.
        }
    }

    public void ConditionalRethrow_SwitchExpression(string message, bool condition)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            var value = condition switch { true => throw e, _ => message };
        }
    }

    public void WhenFilterAliasRethrow_RecursivePattern(string message)
    {
        try { }
        catch (Exception e) when (e is { } alias)
        {
            logger.LogError(message); // Noncompliant - FP: recursive pattern aliases of the caught exception are not tracked.
            throw alias;
        }
    }

    public void WhenFilterNotAnAlias_PropertyPattern(string message)
    {
        try { }
        catch (Exception e) when (e is InvalidOperationException { InnerException: Exception other })
        {
            logger.LogError(message); // Noncompliant
            throw other;
        }
    }
}

public class LabeledJumps
{
    private readonly ILogger logger = new Logger<LabeledJumps>(new NullLoggerFactory());

    public void NoRethrow_LabeledBreak(string message, bool[] flags)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // Noncompliant
            outer:
            foreach (var a in flags)
            {
                foreach (var b in flags)
                {
                    break outer;
                }
            }
        }
    }

    public void ConditionalRethrow_DoWhileLabeledBreak(string message, bool[] flags)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // FN NET-4904
            outer:
            do
            {
                foreach (var flag in flags)
                {
                    if (flag)
                    {
                        break outer;
                    }
                }
                throw;
            } while (flags.Length > 0);
        }
    }

    public void ConditionalRethrow_DoWhileLabeledContinue(string message, bool[] flags)
    {
        try { }
        catch (Exception e)
        {
            logger.LogError(message); // FN NET-4904
            outer:
            do
            {
                foreach (var flag in flags)
                {
                    if (flag)
                    {
                        continue outer;
                    }
                }
                throw e;
            } while (flags.Length > 0);
        }
    }
}
