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
            string passKode = "a"; // Noncompliant {{"kode" detected here, make sure this is not a hard-coded credential.}}
            string passKodeKode = "a"; // Noncompliant
            string passKoDe = "a"; // Compliant

            string x = "kode=a;kode=a"; // Noncompliant
            string x1 = "facal-faire=a;kode=a"; // Noncompliant
            string x2 = @"x\*+?|}{][)(^$.# =something"; // Noncompliant {{"x\*+?|}{][)(^$.#" detected here, make sure this is not a hard-coded credential.}}
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

    public class NoWordBound
    {
        // This used to be an FN because the regex matched on word boundaries.
        public void Method()
        {
            string x = "*=something"; // Noncompliant
        }
    }
}
