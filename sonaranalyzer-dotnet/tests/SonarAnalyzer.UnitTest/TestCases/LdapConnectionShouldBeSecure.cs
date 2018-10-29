using System;
using System.DirectoryServices;

namespace Tests.Diagnostics
{
    class Program
    {
        DirectoryEntry field1 = new DirectoryEntry(); // Compliant
        DirectoryEntry field2;

        DirectoryEntry Property1 { get; set; } = new DirectoryEntry(); // Compliant
        DirectoryEntry Property2 { get; set; }

        void CtorSetsAllowedValue()
        {
            new DirectoryEntry(); // Compliant
            new DirectoryEntry("path", "user", "pass", AuthenticationTypes.Secure); // Compliant
        }

        void CtorSetsNotAllowedValue()
        {
            new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Noncompliant {{Set the 'AuthenticationType' property of this DirectoryEntry to 'AuthenticationTypes.Secure'.}}
        }

        void InitializerSetsAllowedValue()
        {
            new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None) { AuthenticationType = AuthenticationTypes.Secure }; // Compliant
        }

        void InitializerSetsNotAllowedValue()
        {
            new DirectoryEntry() { AuthenticationType = AuthenticationTypes.None }; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            new DirectoryEntry() { }; // Compliant
        }

        void PropertySetsNotAllowedValue()
        {
            var c = new DirectoryEntry();
            c.AuthenticationType = AuthenticationTypes.None; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            field1.AuthenticationType = AuthenticationTypes.None; // Noncompliant
            this.field1.AuthenticationType = AuthenticationTypes.None; // Noncompliant

            Property1.AuthenticationType = AuthenticationTypes.None; // Noncompliant
            this.Property1.AuthenticationType = AuthenticationTypes.None; // Noncompliant
        }

        void PropertySetsAllowedValue(bool foo)
        {
            var c1 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set below
            c1.AuthenticationType = AuthenticationTypes.Secure;

            field1 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set below
            field1.AuthenticationType = AuthenticationTypes.Secure;

            this.field2 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set below
            this.field2.AuthenticationType = AuthenticationTypes.Secure;

            Property1 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set below
            Property1.AuthenticationType = AuthenticationTypes.Secure;

            this.Property2 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set below
            this.Property2.AuthenticationType = AuthenticationTypes.Secure;

            var c2 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Noncompliant, AuthenticationType is set conditionally
            if (foo)
            {
                c2.AuthenticationType = AuthenticationTypes.Secure;
            }

            var c3 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set after the if
            if (foo)
            {
                // do something
            }
            c3.AuthenticationType = AuthenticationTypes.Secure;

            DirectoryEntry c4 = null;
            if (foo)
            {
                c4 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Noncompliant, HttpOnly is not set in the same scope
            }
            c4.AuthenticationType = AuthenticationTypes.Secure;
        }
    }

    class FieldsAndProperties
    {
        private DirectoryEntry field1 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Noncompliant {{Set the 'AuthenticationType' property of this DirectoryEntry to 'AuthenticationTypes.Secure'.}}
//                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        private DirectoryEntry field2 = new DirectoryEntry() { AuthenticationType = AuthenticationTypes.None }; // Noncompliant
        private DirectoryEntry field3 = new DirectoryEntry() { AuthenticationType = AuthenticationTypes.Secure }; // Compliant
        private DirectoryEntry field4;
        private DirectoryEntry field5;
        private DirectoryEntry field6;

        private DirectoryEntry Property0 => new DirectoryEntry { AuthenticationType = AuthenticationTypes.None }; // Noncompliant
        private DirectoryEntry Property1 { get; set; } = new DirectoryEntry { AuthenticationType = AuthenticationTypes.None }; // Noncompliant
        private DirectoryEntry Property3 { get; set; } = new DirectoryEntry() { AuthenticationType = AuthenticationTypes.Secure }; // Compliant
        private DirectoryEntry Property4 { get; set; }
        private DirectoryEntry Property5 { get; set; }
        private DirectoryEntry Property6 { get; set; }

        void Cases()
        {
            field3.AuthenticationType = AuthenticationTypes.None; // Noncompliant

            field4 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set on the next line
            field4.AuthenticationType = AuthenticationTypes.Secure;

            // this
            this.field5 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set on the next line
            this.field5.AuthenticationType = AuthenticationTypes.Secure;

            field6 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Noncompliant

            Property3.AuthenticationType = AuthenticationTypes.None; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            Property4 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set on the next line
            Property4.AuthenticationType = AuthenticationTypes.Secure;

            // this
            this.Property5 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set on the next line
            this.Property5.AuthenticationType = AuthenticationTypes.Secure;

            Property6 = new DirectoryEntry(); // Compliant
        }
    }
}
