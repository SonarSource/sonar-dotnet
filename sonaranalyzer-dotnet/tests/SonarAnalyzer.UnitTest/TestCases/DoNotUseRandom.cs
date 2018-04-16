using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Main()
        {
            var random1 = new Random(); // Noncompliant {{Use a cryptographically strong random number generator (RNG) like 'System.Security.Cryptography.RandomNumberGenerator'.}}
//                        ^^^^^^^^^^^^
            var random2 = new Random(1); // Noncompliant

            var somethingElse = new EventArgs(); // Compliant, not Random

            var secureRandom = RandomNumberGenerator.Create(); // This is how it should be done
        }
    }
}
