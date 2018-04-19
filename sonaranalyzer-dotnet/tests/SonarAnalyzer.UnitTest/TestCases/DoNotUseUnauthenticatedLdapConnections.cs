using System;
using System.DirectoryServices;

namespace Tests.Diagnostics
{
    class LocalVariables
    {
        /// The general cases such as setting fields and properties and returning from methods
        /// are covered by the tests in <see cref="DoNotUseNonHttpCookies" /> and
        /// <see cref="DoNotUseInsecureCookies" />
        private void Cases1()
        {
            DirectoryEntry variable3;
            var variable1 = new DirectoryEntry(); // Compliant
            var variable2 = new DirectoryEntry() { AuthenticationType = AuthenticationTypes.None }; // Noncompliant

            variable1.AuthenticationType = AuthenticationTypes.None; // Noncompliant

            var variable4 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Noncompliant
            var variable5 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None) { AuthenticationType = AuthenticationTypes.Secure }; // Compliant
            var variable6 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, Secure is set next
            variable6.AuthenticationType = AuthenticationTypes.Secure;

            var variable7 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None | AuthenticationTypes.Secure); // Compliant
            var variable8 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant
            variable8.AuthenticationType = AuthenticationTypes.None | AuthenticationTypes.Secure;
        }
    }
}
