using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            string passWord = @"foo"; // Compliant
            string passKode = "a"; // Noncompliant {{'kode' detected in this expression, review this potentially hardcoded credential.}}
            string passKodeKode = "a"; // Noncompliant {{'kode' detected in this expression, review this potentially hardcoded credential.}}
            string passKoDe = "a"; // Compliant

            string x = "kode=a;kode=a"; // Noncompliant {{'kode' detected in this expression, review this potentially hardcoded credential.}}
            string x2 = "facal-faire=a;kode=a"; // Noncompliant {{'facal-faire, kode' detected in this expression, review this potentially hardcoded credential.}}

        }
    }
}
