using System;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

// Method declarations that accept IIdentity or IPrincipal
void AcceptIdentity1(MyIdentity identity) { } // Compliant - FN
void AcceptIdentity2(IIdentity identity) { } // Compliant - FN
void AcceptPrincipal1(MyPrincipal principal) { } // Compliant - FN
void AcceptPrincipal2(IPrincipal principal) { } // Compliant - FN

record MyIdentity : IIdentity // Noncompliant {{Make sure that permissions are controlled safely here.}}
//                  ^^^^^^^^^
{
    public string Name => throw new NotImplementedException();
    public string AuthenticationType => throw new NotImplementedException();
    public bool IsAuthenticated => throw new NotImplementedException();
}

record MyPrincipal : IPrincipal // Noncompliant
{
    public IIdentity Identity => throw new NotImplementedException();
    public bool IsInRole(string role) => throw new NotImplementedException();
}

partial record Record
{
    void CreatingPermissions()
    {
        WindowsIdentity.GetCurrent(); // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        // All instantiations of PrincipalPermission
        PrincipalPermission principalPermission;
        principalPermission = new PrincipalPermission(PermissionState.None); // Noncompliant
        principalPermission = new PrincipalPermission("", ""); // Noncompliant
        principalPermission = new PrincipalPermission("", "", true); // Noncompliant
    }

    void CreatingPrincipalAndIdentity(WindowsIdentity windowsIdentity) // Noncompliant, IIdentity parameter, see another section with tests
    {
        IIdentity identity;
        identity = new MyIdentity(); // Noncompliant, creation of type that implements IIdentity
//                 ^^^^^^^^^^^^^^^^
        identity = new WindowsIdentity(""); // Noncompliant
        IPrincipal principal;
        principal = new MyPrincipal(); // Noncompliant, creation of type that implements IPrincipal
        principal = new WindowsPrincipal(windowsIdentity); // Noncompliant
    }

    void ThreadSecurity(IPrincipal principal) // Noncompliant, IPrincipal parameter, see another section with tests
    {
        Thread.CurrentPrincipal = principal; // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^^^^^
        principal = Thread.CurrentPrincipal; // Noncompliant
    }

    void AppDomainSecurity(AppDomain appDomain, IPrincipal principal) // Noncompliant, IPrincipal parameter, see another section with tests
    {
        appDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal); // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        appDomain.SetThreadPrincipal(principal); // Noncompliant
        appDomain.ExecuteAssembly(""); // Compliant, not one of the tracked methods
    }

    private IIdentity identity;
    public IIdentity Identity
    {
        get => identity;
        set => identity = value; // Compliant, we do not raise for property accessors
    }

    internal partial bool TryParse(IPrincipal p, out int i); // Noncompliant

    internal partial bool TryParse(IPrincipal p, out int i) { i = 0; return true; } // Noncompliant
}
