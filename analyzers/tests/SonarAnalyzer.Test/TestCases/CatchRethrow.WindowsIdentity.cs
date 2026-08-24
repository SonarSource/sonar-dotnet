using System;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace Tests.TestCases
{
    public class CatchRethrowWindowsIdentity
    {
        /// <summary>Verifies that RunImpersonated establishes an unwind boundary.</summary>
        public void RunImpersonated()
        {
            try
            {
                WindowsIdentity.RunImpersonated(SafeAccessTokenHandle.InvalidHandle, () => throw new InvalidOperationException());
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
    }
}
