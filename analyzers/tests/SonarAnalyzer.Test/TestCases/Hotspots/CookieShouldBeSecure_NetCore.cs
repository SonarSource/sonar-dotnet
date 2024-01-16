using System;
using Microsoft.AspNetCore.Http;

namespace Tests.Diagnostics
{
    class Program
    {
        CookieOptions field1 = new CookieOptions(); // Noncompliant
        CookieOptions field2;

        CookieOptions Property1 { get; set; } = new CookieOptions(); // Noncompliant
        CookieOptions Property2 { get; set; }

        void CtorSetsAllowedValue()
        {
            // none
        }

        void CtorSetsNotAllowedValue()
        {
            new CookieOptions(); // Noncompliant {{Make sure creating this cookie without setting the 'Secure' property is safe here.}}
        }

        void InitializerSetsAllowedValue()
        {
            new CookieOptions() { Secure = true };
        }

        void InitializerSetsNotAllowedValue()
        {
            new CookieOptions() { Secure = false }; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            new CookieOptions() { }; // Noncompliant
            new CookieOptions() { HttpOnly = true }; // Noncompliant
        }

        void PropertySetsNotAllowedValue()
        {
            var c = new CookieOptions() { Secure = true };
            c.Secure = false; // Noncompliant
//          ^^^^^^^^^^^^^^^^

            field1.Secure = false; // Noncompliant
            this.field1.Secure = false; // Noncompliant

            Property1.Secure = false; // Noncompliant
            this.Property1.Secure = false; // Noncompliant
        }

        void PropertySetsAllowedValue(bool foo)
        {
            var c1 = new CookieOptions(); // Compliant, Secure is set below
            c1.Secure = true;

            field1 = new CookieOptions(); // Compliant, Secure is set below
            field1.Secure = true;

            this.field2 = new CookieOptions(); // Compliant, Secure is set below
            this.field2.Secure = true;

            Property1 = new CookieOptions(); // Compliant, Secure is set below
            Property1.Secure = true;

            this.Property2 = new CookieOptions(); // Compliant, Secure is set below
            this.Property2.Secure = true;

            var c2 = new CookieOptions(); // Noncompliant, Secure is set conditionally
            if (foo)
            {
                c2.Secure = true;
            }

            var c3 = new CookieOptions(); // Compliant, Secure is set after the if
            if (foo)
            {
                // do something
            }
            c3.Secure = true;

            CookieOptions c4 = null;
            if (foo)
            {
                c4 = new CookieOptions(); // Noncompliant, Secure is not set in the same scope
            }
            c4.Secure = true;
        }
    }
}
