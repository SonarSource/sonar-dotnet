using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    /// The general cases such as setting fields and properties and returning from methods
    /// are covered by the tests in <see cref="DoNotUseNonHttpCookies" /> and
    /// <see cref="DoNotUseInsecureCookies" />
    class Program
    {
        public void Main()
        {
            var a = new AesManaged(); // Noncompliant {{Use Galois/Counter (GCM/NoPadding) 'Mode' instead.}}
//                  ^^^^^^^^^^^^^^^^

            var b = new AesManaged { Mode = CipherMode.CBC }; // Noncompliant
//                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            var b = new AesManaged // Noncompliant
            {
                Mode = CipherMode.CBC
            };

            a.Mode = CipherMode.CBC; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^
            a.Mode = CipherMode.CFB; // Noncompliant
            a.Mode = CipherMode.CTS; // Noncompliant
            a.Mode = CipherMode.ECB; // Noncompliant
            a.Mode = CipherMode.OFB; // Noncompliant
        }
    }
}
