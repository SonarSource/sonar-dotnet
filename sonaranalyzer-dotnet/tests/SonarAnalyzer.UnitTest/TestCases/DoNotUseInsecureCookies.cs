using System;
using System.Web;

namespace Tests.Diagnostics
{
    class FieldsAndProperties
    {
        private HttpCookie field1 = new HttpCookie("c"); // Noncompliant {{Set the 'Secure' property of this cookie to 'true'.}}
//                                  ^^^^^^^^^^^^^^^^^^^
        private HttpCookie field2 = new HttpCookie("c") { Secure = false }; // Noncompliant
        private HttpCookie field3 = new HttpCookie("c") { Secure = true }; // Compliant
        private HttpCookie field4;
        private HttpCookie field5;
        private HttpCookie field6;

        private HttpCookie Property0 => new HttpCookie("c"); // Noncompliant
        private HttpCookie Property1 { get; set; } = new HttpCookie("c"); // Noncompliant
        private HttpCookie Property2 { get; set; } = new HttpCookie("c") { Secure = false }; // Noncompliant
        private HttpCookie Property3 { get; set; } = new HttpCookie("c") { Secure = true }; // Compliant
        private HttpCookie Property4 { get; set; }
        private HttpCookie Property5 { get; set; }
        private HttpCookie Property6 { get; set; }

        void Cases()
        {
            field3.Secure = false; // Noncompliant

            field4 = new HttpCookie("c"); // Compliant, Secure is set on the next line
            field4.Secure = true;

            // this
            this.field5 = new HttpCookie("c"); // Compliant, Secure is set on the next line
            this.field5.Secure = true;

            field6 = new HttpCookie("c"); // Noncompliant

            Property3.Secure = false; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^

            Property4 = new HttpCookie("c"); // Compliant, Secure is set on the next line
            Property4.Secure = true;

            // this
            this.Property5 = new HttpCookie("c"); // Compliant, Secure is set on the next line
            this.Property5.Secure = true;

            Property6 = new HttpCookie("c"); // Noncompliant
        }
    }

    class LocalVariables
    {
        private void Cases1()
        {
            HttpCookie variable3;
            var variable1 = new HttpCookie("c"); // Noncompliant
            var variable2 = new HttpCookie("c") { Secure = false }; // Noncompliant
            variable3 = new HttpCookie("c"); // Noncompliant

            var variable4 = new HttpCookie("c") { Secure = true }; // Compliant
            variable4.Secure = false; // Noncompliant
        }

        private void Cases2()
        {
            Class.Method(new HttpCookie("c")); // Noncompliant
            Class.Method(new HttpCookie("c") { Secure = false }); // Noncompliant
            Class.Method(new HttpCookie("c") { Secure = true }); // Compliant

            new Class(new HttpCookie("c")); // Noncompliant
            new Class(new HttpCookie("c") { Secure = false }); // Noncompliant
        }

        private HttpCookie Return1()
        {
            return new HttpCookie("c"); // Noncompliant
        }

        private HttpCookie Return2()
        {
            return new HttpCookie("c") { Secure = true }; // Compliant
        }

        private void Cases3()
        {
            var variable6 = new HttpCookie("c"); // Compliant
            var variable5 = new HttpCookie("c"); // Noncompliant
            if (false)
            {
                // Do something else
            }
            variable6.Secure = true; // This is few statements away from declaration

            HttpCookie variable7;
            variable7 = new HttpCookie("c"); // Noncompliant, Secure is conditionally set
            if (true)
            {
                variable7.Secure = true;
            }
        }

        private HttpCookie Factory1() =>
            new HttpCookie("c"); // Noncompliant

        private HttpCookie Factory2() =>
            new HttpCookie("c") { Secure = true }; // Compliant
    }

    class InvalidSyntax
    {
        void Initializers()
        {
            var var1 = new HttpCookie( { Secure = true };
            var var2 = new HttpCookie("c") { Secure }; // Noncompliant
            var var3 = new HttpCookie("c") { Secure = "asdasd" }; // Noncompliant
        }
    }

    class Class
    {
        public Class(HttpCookie cookie) { }
        public static void Method(HttpCookie cookie) { }
    }
}
