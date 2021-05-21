using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class Program
    {
        AesManaged field1 = new(); // Noncompliant
        AesManaged Property1 { get; set; } = new(); // Noncompliant

        void CtorSetsAllowedValue()
        {
            // none
        }

        void InitializerSetsNotAllowedValue()
        {
            AesManaged a1 = new() { Mode = CipherMode.CBC }; // Noncompliant
        }

        void PropertySetsNotAllowedValue()
        {
            AesManaged c = new(); // Noncompliant
            c.Mode = CipherMode.CBC; // Noncompliant
        }
    }
}
