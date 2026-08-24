using System;

namespace Tests.TestCases
{
    public class TemporaryContextControls
    {
        /// <summary>Verifies that an ordinary disposable scope does not suppress the rule.</summary>
        public void OrdinaryUsing()
        {
            try
            {
                using (System.IO.File.OpenRead("temporary-file"))
                {
                    throw new InvalidOperationException();
                }
            }
            catch // Noncompliant
            {
                throw;
            }
        }

        /// <summary>Verifies that lookalike user methods do not suppress the rule.</summary>
        public void UserDefinedLookalikes()
        {
            try
            {
                var context = new UserDefinedContext();
                context.Impersonate();
                context.Run();
                try
                {
                    throw new InvalidOperationException();
                }
                finally
                {
                    context.Revert();
                }
            }
            catch // Noncompliant
            {
                throw;
            }
        }

        /// <summary>Verifies that a rethrow after a handling catch remains reported.</summary>
        public void RethrowAfterHandlingCatch()
        {
            try
            {
                throw new InvalidOperationException();
            }
            catch (ArgumentException)
            {
                Console.WriteLine("handled");
            }
            catch // Noncompliant
            {
                throw;
            }
        }
    }

    public sealed class UserDefinedContext
    {
        /// <summary>Imitates an impersonation method.</summary>
        public void Impersonate() { }

        /// <summary>Imitates a context-running method.</summary>
        public void Run() { }

        /// <summary>Imitates a context-restoration method.</summary>
        public void Revert() { }
    }
}
