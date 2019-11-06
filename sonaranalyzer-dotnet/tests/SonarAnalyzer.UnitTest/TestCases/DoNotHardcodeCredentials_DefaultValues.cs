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
            string password = @"foo"; // Noncompliant {{Make sure hard-coded credential is safe.}}
//                 ^^^^^^^^^^^^^^^^^

            string foo, passwd = "a"; // Noncompliant {{Make sure hard-coded credential is safe.}}
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
            string myPassword4 = @"foo"; // Noncompliant
        }

        public void StandardAPI(SecureString secureString, string nonHardcodedPassword, byte[] byteArray, CspParameters cspParams)
        {
            var networkCredential = new NetworkCredential();
            networkCredential.Password = nonHardcodedPassword;
            networkCredential.Domain = "hardcodedDomain";
            new NetworkCredential("username", secureString);
            new NetworkCredential("username", nonHardcodedPassword);
            new NetworkCredential("username", secureString, "domain");
            new NetworkCredential("username", nonHardcodedPassword, "domain");

            new PasswordDeriveBytes(nonHardcodedPassword, byteArray);
            new PasswordDeriveBytes(new byte[] {1}, byteArray);
            new PasswordDeriveBytes(nonHardcodedPassword, byteArray, cspParams);
            new PasswordDeriveBytes(new byte[] {1}, byteArray, cspParams);
            new PasswordDeriveBytes(nonHardcodedPassword, byteArray, "strHashName", 1);
            new PasswordDeriveBytes(new byte[] {1}, byteArray, "strHashName", 1);
            new PasswordDeriveBytes(nonHardcodedPassword, byteArray, "strHashName", 1, cspParams);
            new PasswordDeriveBytes(new byte[] {1}, byteArray, "strHashName", 1, cspParams);

            new NetworkCredential("username", "hardcoded"); // Noncompliant
            new NetworkCredential("username", "hardcoded", "domain"); // Noncompliant
            networkCredential.Password = "hardcoded"; // Noncompliant
            new PasswordDeriveBytes("hardcoded", byteArray); // Noncompliant
            new PasswordDeriveBytes("hardcoded", byteArray, cspParams); // Noncompliant
            new PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1); // Noncompliant
            new PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1, cspParams); // Noncompliant
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
