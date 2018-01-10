using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            string passWord = @"foo"; // Compliant
            string passKode = "a"; // Noncompliant {{Remove hard-coded password(s): 'kode'.}}
            string passKodeKode = "a"; // Noncompliant {{Remove hard-coded password(s): 'kode'.}}
            string passKoDe = "a"; // Compliant

            string x = "kode=a;kode=a"; // Noncompliant {{Remove hard-coded password(s): 'kode'.}}
            string x2 = "facal-faire=a;kode=a"; // Noncompliant {{Remove hard-coded password(s): 'facal-faire, kode'.}}

        }
    }
}
