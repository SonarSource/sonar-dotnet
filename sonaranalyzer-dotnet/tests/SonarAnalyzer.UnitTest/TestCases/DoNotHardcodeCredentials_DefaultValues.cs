using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            string password = @"foo"; // Noncompliant {{'password' detected in this expression, review this potentially hardcoded credential.}}
//                 ^^^^^^^^^^^^^^^^^

            string foo, passwd = "a"; // Noncompliant {{'passwd' detected in this expression, review this potentially hardcoded credential.}}
//                      ^^^^^^^^^^^^

            string foo2 = @"Password=123"; // Noncompliant

            string bar;
            bar = "Password=p"; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^

            foo = "password=";
            foo = "passwordpassword";
            foo = "foo=1;password=1"; // Noncompliant
            foo = "foo=1password=1";
            foo = ""; // Compliant

            string myPassword1 = null;
            string myPassword2 = "";
            string myPassword3 = "        ";
            string myPassword4 = @"foo"; // Noncompliant {{'password' detected in this expression, review this potentially hardcoded credential.}}
        }
    }

    class FalseNegatives
    {
        private string password;

        public void Foo()
        {
            this.password = "foo"; // False Negative
            Configuration.Password = "foo"; // False Negative
            this.password = Configuration.Password = "foo"; // False Negative
        }

        class Configuration
        {
            public static string Password { get; set; }
        }
    }
}
