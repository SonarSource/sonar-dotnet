using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class CustomHashAlgorithm(byte[] array) : HashAlgorithm  // Noncompliant
//               ^^^^^^^^^^^^^^^^^^^
    {
        public override void Initialize() => throw new NotImplementedException();

        protected override void HashCore(byte[] array, int ibStart, int cbSize) => throw new NotImplementedException();

        protected override byte[] HashFinal() => throw new NotImplementedException();
    }

    public class CustomAsymmetricAlgorithm(byte[] array) : AsymmetricAlgorithm { } // Noncompliant

    public class DerivedClassFromCustomAsymmetricAlgorithm(byte[] array) : CustomAsymmetricAlgorithm(array) { } // Noncompliant
}
