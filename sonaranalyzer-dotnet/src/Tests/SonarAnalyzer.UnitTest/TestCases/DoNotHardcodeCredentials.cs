using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            string password = @"foo"; // Noncompliant {{Remove hard-coded password(s): 'password'.}}
//                 ^^^^^^^^^^^^^^^^^

            string foo, passwd = "a"; // Noncompliant{{Remove hard-coded password(s): 'passwd'.}}
//                      ^^^^^^^^^^^^

            string foo2 = @"Password=123"; // Noncompliant

            string bar;
            bar = "Password=p"; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^

            foo = "password=";
            foo = "passwordpassword";
            foo = "foo=1;password=1"; // Noncompliant
            foo = "foo=1password=1";

            string passWord = @"foo"; // Compliant
            string passKode = "a"; // Noncompliant{{Remove hard-coded password(s): 'kode'.}}
            string passKodeKode = "a"; // Noncompliant{{Remove hard-coded password(s): 'kode'.}}
            string passKoDe = "a"; // Compliant

            string x = "kode=a;kode=a"; // Noncompliant{{Remove hard-coded password(s): 'kode'.}}
            string x2 = "facal-faire=a;kode=a"; // Noncompliant{{Remove hard-coded password(s): 'facal-faire, kode'.}}

        }
    }
}
