using System;
using System.Net;
using System.Security;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            string passWord = @"foo"; // Compliant
            string passKode = "a"; // Noncompliant {{Make sure hard-coded credential is safe.}}
            string passKodeKode = "a"; // Noncompliant
            string passKoDe = "a"; // Compliant

            string x = "kode=a;kode=a"; // Noncompliant
            string x1 = "facal-faire=a;kode=a"; // Noncompliant
            string x2 = @"x\*+?|}{][)(^$.# =something"; // Noncompliant
        }

        public void StandardAPI(SecureString secureString, string nonHardcodedPassword, byte[] byteArray, CspParameters cspParams)
        {
            var networkCredential = new NetworkCredential();
            networkCredential.Password = nonHardcodedPassword;
            networkCredential.Domain = "hardcodedDomain";
            new NetworkCredential("username", secureString);
            new NetworkCredential("username", nonHardcodedPassword, "domain");
            new PasswordDeriveBytes(nonHardcodedPassword, byteArray);
            new PasswordDeriveBytes(new byte[] {1}, byteArray, cspParams);

            new NetworkCredential("username", "hardcoded"); // Noncompliant
            networkCredential.Password = "hardcoded"; // Noncompliant
            new PasswordDeriveBytes("hardcoded", byteArray, cspParams); // Noncompliant
        }
    }

    public class FalseNegatives
    {
        public void Method()
        {
            string x = "*=something"; // FN - current regex expects \b (word boundary) at the beginning
        }
    }
}
