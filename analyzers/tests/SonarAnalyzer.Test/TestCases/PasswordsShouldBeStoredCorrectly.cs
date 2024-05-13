using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

class Testcases
{
    const int ITERATIONS = 100_000;
    const PasswordHasherCompatibilityMode MODE = PasswordHasherCompatibilityMode.IdentityV3;

    void PasswordHasherOptions_IterationCount(int iterations)
    {
        var sut = new PasswordHasherOptions()
        {
            IterationCount = 100_000        // Compliant
        };

        sut = new PasswordHasherOptions()
        {
            IterationCount = 1                  // Noncompliant
        //  ^^^^^^^^^^^^^^
        };

        sut.IterationCount = -1;                // Noncompliant
        //  ^^^^^^^^^^^^^^
        sut.IterationCount = 42;                // Noncompliant
        //  ^^^^^^^^^^^^^^
        sut.IterationCount = 100_000 - 1;       // Noncompliant
        //  ^^^^^^^^^^^^^^
        sut.IterationCount = ITERATIONS - 42;   // Noncompliant
        //  ^^^^^^^^^^^^^^

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
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2  // Noncompliant
                                                                            //  ^^^^^^^^^^^^^^^^^
        };

        sut.CompatibilityMode = MODE;                                       // Compliant
        sut.CompatibilityMode = (PasswordHasherCompatibilityMode)1;         // Compliant
        sut.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3; // Compliant

        sut.CompatibilityMode = 0;                                          // Noncompliant
        //  ^^^^^^^^^^^^^^^^^
        sut.CompatibilityMode = (PasswordHasherCompatibilityMode)0;         // Noncompliant
        //  ^^^^^^^^^^^^^^^^^
        sut.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2; // Noncompliant
        //  ^^^^^^^^^^^^^^^^^

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
        KeyDerivation.Pbkdf2(null, null, 0, 42, 0);                                 // Noncompliant
        //                                  ^^
        KeyDerivation.Pbkdf2(null, null, 0, iterationCount: 100_000 - 42, 0);       // Noncompliant
        //                                  ^^^^^^^^^^^^^^^^^^^^^^^
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

    // TODO: Finish this
    void Rfc2898DeriveBytes_IterationCount(int iterations, byte[] bs)
    {
        new Rfc2898DeriveBytes("password", bs);                             // Noncompliant
    //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        new Rfc2898DeriveBytes("password", 42);                             // Noncompliant
    //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        new Rfc2898DeriveBytes(bs, bs, 42);                                 // Noncompliant
    //                                 ^^
        new Rfc2898DeriveBytes("password", bs, 42);                         // Noncompliant
    //                                         ^^
        new Rfc2898DeriveBytes("password", 42, 42);                         // Noncompliant
    //                                         ^^
        new Rfc2898DeriveBytes(bs,bs, 42, HashAlgorithmName.MD5);           // Noncompliant
    //                                ^^
        new Rfc2898DeriveBytes("password",bs, 42, HashAlgorithmName.MD5);   // Noncompliant
    //                                        ^^
        new Rfc2898DeriveBytes("password",42, 42, HashAlgorithmName.MD5);   // Noncompliant
    //                                        ^^
    }
}
