using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Main()
        {
            new Random(); // Noncompliant {{Make sure that using this pseudorandom number generator is safe here.}}
//          ^^^^^^^^^^^^
            new Random(1); // Noncompliant

            new EventArgs(); // Compliant, not Random

            RandomNumberGenerator.Create(); // Compliant, using cryptographically strong RNG
        }
    }
}
