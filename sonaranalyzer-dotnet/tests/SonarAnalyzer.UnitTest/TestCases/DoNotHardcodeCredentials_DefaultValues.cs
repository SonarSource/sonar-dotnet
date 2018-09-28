using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            string password = @"foo"; // Noncompliant {{Remove hard-coded password(s): 'password'.}}
//                 ^^^^^^^^^^^^^^^^^

            string foo, passwd = "a"; // Noncompliant {{Remove hard-coded password(s): 'passwd'.}}
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
