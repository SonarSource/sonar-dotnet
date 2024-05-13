using System;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;

class Testcases
{
    const int ITERATIONS = 100_000;
    const PasswordHasherCompatibilityMode MODE = PasswordHasherCompatibilityMode.IdentityV3;

    void PasswordHasherOptions_IterationCount(int iterations)
    {
        var sut = new PasswordHasherOptions()
        {
            IterationCount = 100_000            // Compliant
        };

        sut = new PasswordHasherOptions()
        {
            IterationCount = 1                  // Noncompliant {{Use at least 100,000 iterations here.}}
    //      ^^^^^^^^^^^^^^
        };

        sut.IterationCount = -1;                // Noncompliant {{Use at least 100,000 iterations here.}}
    //  ^^^^^^^^^^^^^^^^^^
        sut.IterationCount = 42;                // Noncompliant
        sut.IterationCount = 100_000 - 1;       // Noncompliant
        sut.IterationCount = ITERATIONS - 42;   // Noncompliant

        sut.IterationCount = ITERATIONS;        // Compliant
        sut.IterationCount = 100_000;           // Compliant
        sut.IterationCount = 1_000_000;         // Compliant

        if (iterations >= 100_000)
        {
            sut.IterationCount = iterations;    // Compliant
        }
        else
        {
            sut.IterationCount = iterations;    // Compliant FN
        }

        SetIterationCount(42);                  // Compliant FN

        void SetIterationCount(int value)
        {
            sut.IterationCount = value;
        }
    }

    void PasswordHasherOptions_CompatibilityMode(PasswordHasherCompatibilityMode mode)
    {
        var sut = new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3  // Compliant
        };

        sut = new PasswordHasherOptions()
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2  // Noncompliant {{Identity v2 uses only 1000 iterations. Consider changing to identity V3.}}
    //      ^^^^^^^^^^^^^^^^^
        };

        sut.CompatibilityMode = MODE;                                       // Compliant
        sut.CompatibilityMode = (PasswordHasherCompatibilityMode)1;         // Compliant
        sut.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3; // Compliant

        sut.CompatibilityMode = 0;                                          // Noncompliant {{Identity v2 uses only 1000 iterations. Consider changing to identity V3.}}
    //  ^^^^^^^^^^^^^^^^^^^^^
        sut.CompatibilityMode = (PasswordHasherCompatibilityMode)0;         // Noncompliant
        sut.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2; // Noncompliant

        if (mode == PasswordHasherCompatibilityMode.IdentityV3)
        {
            sut.CompatibilityMode = mode;                                   // Compliant
        }
        else
        {
            sut.CompatibilityMode = mode;                                   // Compliant FN
        }

        SetCompatibility(PasswordHasherCompatibilityMode.IdentityV2);       // Compliant FN

        void SetCompatibility(PasswordHasherCompatibilityMode value)
        {
            sut.CompatibilityMode = value;
        }
    }

    void KeyDerivation_Pbkdf2(int iterations)
    {
        KeyDerivation.Pbkdf2(null, null, 0, 42, 0);                                 // Noncompliant {{Use at least 100,000 iterations here.}}
        //                                  ^^
        KeyDerivation.Pbkdf2(null, null, 0, iterationCount: 100_000 - 42, 0);       // Noncompliant
        KeyDerivation.Pbkdf2(null, null, 0, iterationCount: ITERATIONS - 42, 0);    // Noncompliant

        KeyDerivation.Pbkdf2(
            iterationCount: 42,                                                     // Noncompliant {{Use at least 100,000 iterations here.}}
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

    void Rfc2898DeriveBytes_Pbkdf2(int iterations, byte[] bs, HashAlgorithmName han, ReadOnlySpan<byte> ss, ReadOnlySpan<char> cs, Span<byte> dest)
    {
        Rfc2898DeriveBytes.Pbkdf2(bs, bs, 42, han, 42);                     // Noncompliant {{Use at least 100,000 iterations here.}}
        Rfc2898DeriveBytes.Pbkdf2("", bs, 42, han, 42);                     // Noncompliant
        Rfc2898DeriveBytes.Pbkdf2(ss, ss, 42, han, 42);                     // Noncompliant
        Rfc2898DeriveBytes.Pbkdf2(ss, ss, dest, 42, han);                   // Noncompliant
        Rfc2898DeriveBytes.Pbkdf2(cs, ss, 42, han, 42);                     // Noncompliant
        Rfc2898DeriveBytes.Pbkdf2(cs, ss, dest, 42, han);                   // Noncompliant
    //                                          ^^

        Rfc2898DeriveBytes.Pbkdf2(
            iterations: 42,                                                 // Noncompliant
       //   ^^^^^^^^^^^^^^
            password: cs,
            salt: ss,
            hashAlgorithm: han,
            outputLength: 0);

        Rfc2898DeriveBytes.Pbkdf2(bs, bs, ITERATIONS, han, 42);             // Compliant
        Rfc2898DeriveBytes.Pbkdf2("", bs, ITERATIONS, han, 42);             // Compliant
        Rfc2898DeriveBytes.Pbkdf2(ss, ss, ITERATIONS, han, 42);             // Compliant
        Rfc2898DeriveBytes.Pbkdf2(ss, ss, dest, ITERATIONS, han);           // Compliant
        Rfc2898DeriveBytes.Pbkdf2(cs, ss, ITERATIONS, han, 42);             // Compliant
        Rfc2898DeriveBytes.Pbkdf2(cs, ss, dest, ITERATIONS, han);           // Compliant
    }
}
