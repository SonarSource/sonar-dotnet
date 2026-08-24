using System;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;

namespace Tests.TestCases
{
    public class CatchRethrowLegacyContexts
    {
        /// <summary>Verifies that an impersonation using scope establishes an unwind boundary.</summary>
        public void LegacyImpersonation()
        {
            try
            {
                using (WindowsIdentity.GetCurrent().Impersonate())
                {
                    throw new InvalidOperationException();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Verifies that an assertion restored in an inner finally establishes an unwind boundary.</summary>
        public void CodeAccessPermissionAssertion()
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.Execution).Assert();
                try
                {
                    throw new InvalidOperationException();
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
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
