using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.TestCases
{

class CatchRethrowTemporaryContextNet
{
    void ExecutionContextRun()
    {
        try
        {
            ExecutionContext.Run(
                ExecutionContext.Capture(),
                _ => throw new InvalidOperationException(),
                null);
        }
        catch // Compliant: the captured context is restored before an upstream exception filter runs
        {
            throw;
        }
    }

    void RunImpersonated()
    {
        try
        {
            WindowsIdentity.RunImpersonated(null, () => throw new InvalidOperationException());
        }
        catch // Compliant: impersonation is restored before an upstream exception filter runs
        {
            throw;
        }
    }

    async Task RunImpersonatedAsync()
    {
        try
        {
            await WindowsIdentity.RunImpersonatedAsync(null, async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException();
            });
        }
        catch // Noncompliant
        {
            throw;
        }
    }

    void NestedExecutionContextRun()
    {
        try
        {
            void RunInLocalFunction() =>
                ExecutionContext.Run(
                    ExecutionContext.Capture(),
                    _ => throw new InvalidOperationException(),
                    null);

            throw new InvalidOperationException();
        }
        catch // Noncompliant
        {
            throw;
        }
    }

    void HandledInnerExecutionContextRun()
    {
        try
        {
            try
            {
                ExecutionContext.Run(
                    ExecutionContext.Capture(),
                    _ => throw new InvalidOperationException(),
                    null);
            }
            catch
            {
            }

            throw new InvalidOperationException();
        }
        catch // Noncompliant
        {
            throw;
        }
    }

    void ExceptionHandledInnerExecutionContextRun()
    {
        try
        {
            try
            {
                ExecutionContext.Run(
                    ExecutionContext.Capture(),
                    _ => throw new InvalidOperationException(),
                    null);
            }
            catch (Exception)
            {
            }

            throw new InvalidOperationException();
        }
        catch // Noncompliant
        {
            throw;
        }
    }

    void ExecutionContextRunInInnerCatch()
    {
        try
        {
            try
            {
                throw new InvalidOperationException();
            }
            catch
            {
                ExecutionContext.Run(
                    ExecutionContext.Capture(),
                    _ => throw new InvalidOperationException(),
                    null);
            }
        }
        catch // Compliant: the inner catch does not handle exceptions thrown from its own body
        {
            throw;
        }
    }

    void ExecutionContextRunInInnerFinally()
    {
        try
        {
            try
            {
                throw new InvalidOperationException();
            }
            catch
            {
            }
            finally
            {
                ExecutionContext.Run(
                    ExecutionContext.Capture(),
                    _ => throw new InvalidOperationException(),
                    null);
            }
        }
        catch // Compliant: the inner catch does not handle exceptions thrown from its finally block
        {
            throw;
        }
    }

    void TypedCatchAllIsABoundary()
    {
        try
        {
            ExecutionContext.Run(
                ExecutionContext.Capture(),
                _ => throw new InvalidOperationException(),
                null);
        }
        catch (Exception) // Compliant: the captured context is restored before an upstream exception filter runs
        {
            throw;
        }
    }

    void SpecificCatchIsNotABoundary()
    {
        try
        {
            ExecutionContext.Run(
                ExecutionContext.Capture(),
                _ => throw new InvalidOperationException(),
                null);
        }
        catch (InvalidOperationException) // Noncompliant
        {
            throw;
        }
    }
}
}
