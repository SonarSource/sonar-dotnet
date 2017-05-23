using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            string password = @"foo"; // Noncompliant {{Remove this hard-coded password.}}
//                 ^^^^^^^^^^^^^^^^^

            string foo, passwd = "a";// Noncompliant
//                      ^^^^^^^^^^^^

            string foo2 = @"Password=123"; // Noncompliant

            string bar;
            bar = "Password=p"; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^

            foo = "password=";
            foo = "passwordpassword";
            foo = "foo=1;password=1"; // Noncompliant
            foo = "foo=1password=1";
        }
    }
}
