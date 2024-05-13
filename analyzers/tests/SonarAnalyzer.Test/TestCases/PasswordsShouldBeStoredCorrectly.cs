using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

class Testcases
{
    const int ITERATIONS = 100_000;
    const PasswordHasherCompatibilityMode MODE = PasswordHasherCompatibilityMode.IdentityV3;

    void KeyDerivation_Pbkdf2(int iterations)
    {
        KeyDerivation.Pbkdf2(null, null, 0, 42, 0);                                 // Noncompliant
        //                                  ^^
        KeyDerivation.Pbkdf2(null, null, 0, iterationCount: 100_000 - 42, 0);       // Noncompliant
        //                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        KeyDerivation.Pbkdf2(null, null, 0, iterationCount: ITERATIONS - 42, 0);    // Noncompliant
        //                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        KeyDerivation.Pbkdf2(
            iterationCount: 42,                                                     // Noncompliant
        //  ^^^^^^^^^^^^^^^^^^
            password: "exploding",
            salt: Encoding.UTF8.GetBytes("whale"),
            numBytesRequested: 42,
            prf: 0);

        KeyDerivation.Pbkdf2(null, null, 0, 100_000, 0);                            // Compliant
        KeyDerivation.Pbkdf2(null, null, 0, iterationCount: 100_000 - 42 + 43, 0);  // Compliant
        KeyDerivation.Pbkdf2(null, null, 0, iterationCount: ITERATIONS, 0);         // Compliant
        KeyDerivation.Pbkdf2(null, null, 0, iterationCount: iterations, 0);         // Compliant

        KeyDerivation.Pbkdf2(
            iterationCount: 100_000,                                                // Compliant
            password: "exploding",
            salt: Encoding.UTF8.GetBytes("whale"),
            numBytesRequested: 42,
            prf: 0);
    }

}
