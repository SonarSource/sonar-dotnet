using System;
using System.Security;
using System.Security.Principal;

namespace Tests.TestCases
{

class CatchRethrowTemporaryContextNetFramework
{
    void LegacyImpersonation(WindowsIdentity identity)
    {
        try
        {
            using (identity.Impersonate())
            {
                throw new InvalidOperationException();
            }
        }
        catch // Compliant: the using scope restores the identity before an upstream exception filter runs
        {
            throw;
        }
    }

    void ParenthesizedLegacyImpersonation(WindowsIdentity identity)
    {
        try
        {
            using ((identity.Impersonate()))
            {
                throw new InvalidOperationException();
            }
        }
        catch
        {
            throw;
        }
    }

    void CastLegacyImpersonation(WindowsIdentity identity)
    {
        try
        {
            using ((IDisposable)identity.Impersonate())
            {
                throw new InvalidOperationException();
            }
        }
        catch
        {
            throw;
        }
    }

    void PermissionAssertion(CodeAccessPermission permission)
    {
        try
        {
            try
            {
                permission.Assert();
                throw new InvalidOperationException();
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }
        catch // Compliant: the nested finally reverts the assertion before the exception leaves this frame
        {
            throw;
        }
    }

    void SameFramePermissionAssertion(CodeAccessPermission permission)
    {
        try
        {
            permission.Assert();
            throw new InvalidOperationException();
        }
        catch // Noncompliant
        {
            throw;
        }
        finally
        {
            CodeAccessPermission.RevertAssert();
        }
    }

    void NestedImpersonationInUsingStatement(WindowsIdentity identity)
    {
        try
        {
            using (Ignore(identity.Impersonate()))
            {
                throw new InvalidOperationException();
            }
        }
        catch // Noncompliant
        {
            throw;
        }
    }

    private static IDisposable Ignore(WindowsImpersonationContext _) =>
        new EmptyDisposable();

    private sealed class EmptyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
}
