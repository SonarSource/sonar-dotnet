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
            new CookieOptions(); // Noncompliant {{Set the 'HttpOnly' property of this cookie to 'true'.}}
        }

        void InitializerSetsAllowedValue()
        {
            new CookieOptions() { HttpOnly = true };
        }

        void InitializerSetsNotAllowedValue()
        {
            new CookieOptions() { HttpOnly = false }; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            new CookieOptions() { }; // Noncompliant
            new CookieOptions() { Secure = true }; // Noncompliant
        }

        void PropertySetsNotAllowedValue()
        {
            var c = new CookieOptions() { HttpOnly = true };
            c.HttpOnly = false; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^

            field1.HttpOnly = false; // Noncompliant
            this.field1.HttpOnly = false; // Noncompliant

            Property1.HttpOnly = false; // Noncompliant
            this.Property1.HttpOnly = false; // Noncompliant
        }

        void PropertySetsAllowedValue(bool foo)
        {
            var c1 = new CookieOptions(); // Compliant, HttpOnly is set below
            c1.HttpOnly = true;

            field1 = new CookieOptions(); // Compliant, HttpOnly is set below
            field1.HttpOnly = true;

            this.field2 = new CookieOptions(); // Compliant, HttpOnly is set below
            this.field2.HttpOnly = true;

            Property1 = new CookieOptions(); // Compliant, HttpOnly is set below
            Property1.HttpOnly = true;

            this.Property2 = new CookieOptions(); // Compliant, HttpOnly is set below
            this.Property2.HttpOnly = true;

            var c2 = new CookieOptions(); // Noncompliant, HttpOnly is set conditionally
            if (foo)
            {
                c2.HttpOnly = true;
            }

            var c3 = new CookieOptions(); // Compliant, HttpOnly is set after the if
            if (foo)
            {
                // do something
            }
            c3.HttpOnly = true;

            CookieOptions c4 = null;
            if (foo)
            {
                c4 = new CookieOptions(); // Noncompliant, HttpOnly is not set in the same scope
            }
            c4.HttpOnly = true;
        }
    }
}
