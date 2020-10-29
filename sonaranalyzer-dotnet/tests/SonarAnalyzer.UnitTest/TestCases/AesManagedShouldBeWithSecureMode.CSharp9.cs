using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class Program
    {
        AesManaged field1 = new(); // FN
        AesManaged Property1 { get; set; } = new(); // FN

        void CtorSetsAllowedValue()
        {
            // none
        }

        void InitializerSetsNotAllowedValue()
        {
            AesManaged a1 = new() { Mode = CipherMode.CBC }; // FN
        }

        void PropertySetsNotAllowedValue()
        {
            AesManaged c = new(); // FN
            c.Mode = CipherMode.CBC; // Noncompliant
        }
    }
}
