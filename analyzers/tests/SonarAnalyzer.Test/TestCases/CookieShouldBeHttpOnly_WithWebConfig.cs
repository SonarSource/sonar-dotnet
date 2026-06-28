using System;
using System.Web;

namespace Tests.Diagnostics
{
    class Program
    {
        HttpCookie field1 = new HttpCookie("c"); // Compliant, Web.Config overrides the default behaviour.
        HttpCookie field2;

        HttpCookie Property1 { get; set; } = new HttpCookie("c"); // Compliant
        HttpCookie Property2 { get; set; }

        private bool trueField = true;
        private bool falseField = false;

        void CtorSetsAllowedValue()
        {
            new HttpCookie("c"); // Compliant
        }

        void InitializerSetsAllowedValue()
        {
            new HttpCookie("c") { HttpOnly = true };
            new HttpCookie("c") { HttpOnly = trueField };
        }

        void InitializerSetsNotAllowedValue()
        {
            new HttpCookie("c") { HttpOnly = false };       // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            new HttpCookie("c") { HttpOnly = falseField };  // Noncompliant
            new HttpCookie("c") { };                        // Compliant
            new HttpCookie("c") { Secure = true };          // Compliant
        }

        void PropertySetsNotAllowedValue()
        {
            var c = new HttpCookie("c") { HttpOnly = true };
            c.HttpOnly = false; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^

            field1.HttpOnly = false;        // Noncompliant
            field1.HttpOnly = falseField;   // Noncompliant
            this.field1.HttpOnly = false;   // Noncompliant

            Property1.HttpOnly = false;         // Noncompliant
            this.Property1.HttpOnly = false;    // Noncompliant
        }

        void PropertySetsAllowedValue(bool foo)
        {
            var c1 = new HttpCookie("c"); // Compliant, HttpOnly is set below
            c1.HttpOnly = true;

            var cc = new HttpCookie("c"); // Compliant, HttpOnly is set below
            cc.HttpOnly = trueField;

            field1 = new HttpCookie("c"); // Compliant, HttpOnly is set below
            field1.HttpOnly = true;

            this.field2 = new HttpCookie("c"); // Compliant, HttpOnly is set below
            this.field2.HttpOnly = true;

            Property1 = new HttpCookie("c"); // Compliant, HttpOnly is set below
            Property1.HttpOnly = true;

            this.Property2 = new HttpCookie("c"); // Compliant, HttpOnly is set below
            this.Property2.HttpOnly = true;

            var c2 = new HttpCookie("c"); // Compliant
            if (foo)
            {
                c2.HttpOnly = true;
            }

            var c3 = new HttpCookie("c"); // Compliant, HttpOnly is set after the if
            if (foo)
            {
                // do something
            }
            c3.HttpOnly = true;

            HttpCookie c4 = null;
            if (foo)
            {
                c4 = new HttpCookie("c"); // Compliant
            }
            c4.HttpOnly = true;
        }

        void RaiseTwice()
        {
            var x = new HttpCookie("c"); // Compliant
            x.HttpOnly = false; // Noncompliant
        }
    }
}
