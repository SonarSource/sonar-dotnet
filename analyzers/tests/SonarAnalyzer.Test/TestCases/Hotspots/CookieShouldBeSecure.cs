using System;
using System.Web;

namespace Tests.Diagnostics
{
    class Program
    {
        HttpCookie field1 = new HttpCookie("c"); // Noncompliant
        HttpCookie field2;
        HttpCookie field3 = new HttpCookie("c") { Secure = "false" }; // Noncompliant
        // Error@-1 [CS0029]

        HttpCookie Property1 { get; set; } = new HttpCookie("c"); // Noncompliant
        HttpCookie Property2 { get; set; }

        private bool trueField = true;
        private bool falseField = false;

        void CtorSetsAllowedValue()
        {
            // none
        }

        void CtorSetsNotAllowedValue()
        {
            new HttpCookie("c"); // Noncompliant {{Make sure creating this cookie without setting the 'Secure' property is safe here.}}
        }

        void InitializerSetsAllowedValue()
        {
            new HttpCookie("c") { Secure = true };
            new HttpCookie("c") { Secure = trueField };
        }

        void InitializerSetsNotAllowedValue()
        {
            new HttpCookie("c") { Secure = false };         // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            new HttpCookie("c") { Secure = falseField };    // Noncompliant
            new HttpCookie("c") { };                        // Noncompliant
            new HttpCookie("c") { HttpOnly = true };        // Noncompliant
        }

        void PropertySetsNotAllowedValue()
        {
            var c = new HttpCookie("c") { Secure = true };
            c.Secure = false; // Noncompliant
//          ^^^^^^^^^^^^^^^^

            field1.Secure = false; // Noncompliant
            this.field1.Secure = false; // Noncompliant

            Property1.Secure = false; // Noncompliant
            this.Property1.Secure = false; // Noncompliant
        }

        void PropertySetsAllowedValue(bool foo)
        {
            var c1 = new HttpCookie("c"); // Compliant, Secure is set below
            c1.Secure = true;

            field1 = new HttpCookie("c"); // Compliant, Secure is set below
            field1.Secure = true;

            this.field2 = new HttpCookie("c"); // Compliant, Secure is set below
            this.field2.Secure = true;

            Property1 = new HttpCookie("c"); // Compliant, Secure is set below
            Property1.Secure = true;

            this.Property2 = new HttpCookie("c"); // Compliant, Secure is set below
            this.Property2.Secure = true;

            var c2 = new HttpCookie("c"); // Noncompliant, Secure is set conditionally
            if (foo)
            {
                c2.Secure = true;
            }

            var c3 = new HttpCookie("c"); // Compliant, Secure is set after the if
            if (foo)
            {
                // do something
            }
            c3.Secure = true;

            HttpCookie c4 = null;
            if (foo)
            {
                c4 = new HttpCookie("c"); // Noncompliant, Secure is not set in the same scope
            }
            c4.Secure = true;
        }
    }
}
