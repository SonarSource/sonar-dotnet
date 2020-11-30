using System;
using System.IdentityModel.Tokens;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace Tests.Diagnostics
{
    class Program
    {
        class MyIdentity : IIdentity // Noncompliant {{Make sure that permissions are controlled safely here.}}
//                         ^^^^^^^^^
        {
            public string Name => throw new NotImplementedException();
            public string AuthenticationType => throw new NotImplementedException();
            public bool IsAuthenticated => throw new NotImplementedException();
        }

        class MyPrincipal : IPrincipal // Noncompliant
        {
            public IIdentity Identity => throw new NotImplementedException();
            public bool IsInRole(string role) => throw new NotImplementedException();
        }

        // Indirectly implementing IIdentity
        class MyWindowsIdentity : WindowsIdentity // Noncompliant
        {
            public MyWindowsIdentity() : base("") { }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Administrators")]
        void SecuredMethod() { } // Noncompliant, decorated with PrincipalPermission
//           ^^^^^^^^^^^^^

        void ValidateSecurityToken(SecurityTokenHandler handler, SecurityToken securityToken)
        {
            handler.ValidateToken(securityToken); // Noncompliant
        }

        void CreatingPermissions()
        {
            WindowsIdentity.GetCurrent(); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            // All instantiations of PrincipalPermission
            PrincipalPermission principalPermission;
            principalPermission = new PrincipalPermission(PermissionState.None); // Noncompliant
            principalPermission = new PrincipalPermission("", ""); // Noncompliant
            principalPermission = new PrincipalPermission("", "", true); // Noncompliant
        }

        void HttpContextUser(HttpContext httpContext)
        {
            var user = httpContext.User; // Noncompliant
//                     ^^^^^^^^^^^^^^^^
            httpContext.User = user; // Noncompliant
//          ^^^^^^^^^^^^^^^^
        }

        void AppDomainSecurity(AppDomain appDomain, IPrincipal principal) // Noncompliant, IPrincipal parameter, see another section with tests
        {
            appDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            appDomain.SetThreadPrincipal(principal); // Noncompliant
            appDomain.ExecuteAssembly(""); // Compliant, not one of the tracked methods
        }

        void ThreadSecurity(IPrincipal principal) // Noncompliant, IPrincipal parameter, see another section with tests
        {
            Thread.CurrentPrincipal = principal; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^
            principal = Thread.CurrentPrincipal; // Noncompliant
        }

        void CreatingPrincipalAndIdentity(WindowsIdentity windowsIdentity) // Noncompliant, IIdentity parameter, see another section with tests
        {
            IIdentity identity;
            identity = new MyIdentity(); // Noncompliant, creation of type that implements IIdentity
//                     ^^^^^^^^^^^^^^^^
            identity = new WindowsIdentity(""); // Noncompliant
            IPrincipal principal;
            principal = new MyPrincipal(); // Noncompliant, creation of type that implements IPrincipal
            principal = new WindowsPrincipal(windowsIdentity); // Noncompliant
        }

        // Method declarations that accept IIdentity or IPrincipal
        void AcceptIdentity(MyIdentity identity) { } // Noncompliant
//           ^^^^^^^^^^^^^^
        void AcceptIdentity(IIdentity identity) { } // Noncompliant
        void AcceptPrincipal(MyPrincipal principal) { } // Noncompliant
        void AcceptPrincipal(IPrincipal principal) { } // Noncompliant
    }

    public class Properties
    {
        private IIdentity identity;

        public IIdentity Identity
        {
            get
            {
                return identity;
            }
            set // Compliant, we do not raise for property accessors
            {
                identity = value;
            }
        }
    }
}
