using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Main()
        {
            new Random(); // Noncompliant {{Use a cryptographically strong random number generator (RNG) like 'System.Security.Cryptography.RandomNumberGenerator'.}}
//          ^^^^^^^^^^^^
            new Random(1); // Noncompliant

            new EventArgs(); // Compliant, not Random

            RandomNumberGenerator.Create(); // Compliant, using cryptographically strong RNG
        }
    }
}
