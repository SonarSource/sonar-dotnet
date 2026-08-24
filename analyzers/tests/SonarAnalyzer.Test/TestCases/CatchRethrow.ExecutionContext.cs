using System;
using System.Threading;

namespace Tests.TestCases
{
    public class CatchRethrowExecutionContext
    {
        /// <summary>Verifies that ExecutionContext.Run establishes an unwind boundary.</summary>
        public void ExecutionContextRun()
        {
            try
            {
                ExecutionContext.Run(ExecutionContext.Capture(), _ => throw new InvalidOperationException(), null);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Verifies that an unrelated rethrow is still reported.</summary>
        public void UnrelatedRethrow()
        {
            try
            {
                throw new InvalidOperationException();
            }
            catch // Noncompliant
            {
                throw;
            }
        }

        /// <summary>Verifies that only the local temporary-context boundary is exempted.</summary>
        public void NestedTemporaryContext()
        {
            try
            {
                try
                {
                    ExecutionContext.Run(ExecutionContext.Capture(), _ => throw new InvalidOperationException(), null);
                }
                catch
                {
                    throw;
                }
            }
            catch // Noncompliant
            {
                throw;
            }
        }
    }
}
