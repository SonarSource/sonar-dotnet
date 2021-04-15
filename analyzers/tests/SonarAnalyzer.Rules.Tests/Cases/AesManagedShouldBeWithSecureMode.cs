using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class Program
    {
        AesManaged field1 = new AesManaged(); // Noncompliant
        AesManaged Property1 { get; set; } = new AesManaged(); // Noncompliant

        void CtorSetsAllowedValue()
        {
            // none
        }

        void CtorSetsNotAllowedValue()
        {
            new AesManaged(); // Noncompliant {{Use a certified third party library implementing Galois/Counter Mode (GCM) instead.}}
        }

        void InitializerSetsAllowedValue()
        {
            // none
        }

        void InitializerSetsNotAllowedValue()
        {
            new AesManaged() { Mode = CipherMode.CBC }; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            new AesManaged() { Mode = CipherMode.CFB }; // Noncompliant
            new AesManaged() { Mode = CipherMode.CTS }; // Noncompliant
            new AesManaged() { Mode = CipherMode.ECB }; // Noncompliant
            new AesManaged() { Mode = CipherMode.OFB }; // Noncompliant
        }

        void PropertySetsNotAllowedValue()
        {
            var c = new AesManaged(); // Noncompliant
            c.Mode = CipherMode.CBC; // Noncompliant, we will be raising twice
            c.Mode = CipherMode.CFB; // Noncompliant, we will be raising twice
            c.Mode = CipherMode.CTS; // Noncompliant, we will be raising twice
            c.Mode = CipherMode.ECB; // Noncompliant, we will be raising twice
            c.Mode = CipherMode.OFB; // Noncompliant, we will be raising twice
        }

        void PropertySetsAllowedValue(bool foo)
        {
            // none
        }
    }
}
