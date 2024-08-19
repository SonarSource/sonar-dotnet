using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AliasedPasswordDeriveBytes = System.Security.Cryptography.PasswordDeriveBytes;

class Program
{
    private const string passwordString = "Secret";
    private CspParameters cspParams = new CspParameters();
    private readonly byte[] passwordBytes = Encoding.UTF8.GetBytes(passwordString);

    // Out of the two issues (salt is too short vs. salt is predictable) being predictable is the more serious one.
    // If both issues are present then the rule's message will reflect on the salt being predictable.
    public void ShortAndConstantSaltIsNotCompliant()
    {
        var shortAndConstantSalt = new byte[15];
        var pdb1 = new PasswordDeriveBytes(passwordBytes, shortAndConstantSalt);                                                    // Noncompliant {{Make this salt unpredictable.}}
        //                                                ^^^^^^^^^^^^^^^^^^^^
        var pdb2 = new PasswordDeriveBytes(salt: shortAndConstantSalt, password: passwordBytes);                                    // Noncompliant
        //                                       ^^^^^^^^^^^^^^^^^^^^
        var pdb3 = new PasswordDeriveBytes(passwordString, shortAndConstantSalt);                                                   // Noncompliant
        var pdb4 = new PasswordDeriveBytes(passwordBytes, shortAndConstantSalt, cspParams);                                         // Noncompliant
        var pdb5 = new PasswordDeriveBytes(passwordString, shortAndConstantSalt, cspParams);                                        // Noncompliant
        var pdb6 = new PasswordDeriveBytes(passwordBytes, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000);               // Noncompliant
        var pdb7 = new PasswordDeriveBytes(passwordString, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000);              // Noncompliant
        var pdb8 = new PasswordDeriveBytes(passwordBytes, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams);    // Noncompliant
        var pdb9 = new PasswordDeriveBytes(passwordString, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams);   // Noncompliant

        var pbkdf2a = new Rfc2898DeriveBytes(passwordString, shortAndConstantSalt);                                                 // Noncompliant {{Make this salt unpredictable.}}
        var pbkdf2b = new Rfc2898DeriveBytes(passwordString, shortAndConstantSalt, 1000);                                           // Noncompliant
        var pbkdf2c = new Rfc2898DeriveBytes(passwordBytes, shortAndConstantSalt, 1000);                                            // Noncompliant
        var pbkdf2d = new Rfc2898DeriveBytes(passwordString, shortAndConstantSalt, 1000, HashAlgorithmName.SHA512);                 // Noncompliant
    }

    public void ConstantHashIsNotCompliant()
    {
        var constantSalt = new byte[16];
        var pdb1 = new PasswordDeriveBytes(passwordBytes, constantSalt);                                                    // Noncompliant {{Make this salt unpredictable.}}
        var pdb2 = new PasswordDeriveBytes(passwordString, constantSalt);                                                   // Noncompliant
        var pdb3 = new PasswordDeriveBytes(passwordBytes, constantSalt, cspParams);                                         // Noncompliant
        var pdb4 = new PasswordDeriveBytes(passwordString, constantSalt, cspParams);                                        // Noncompliant
        var pdb5 = new PasswordDeriveBytes(passwordBytes, constantSalt, HashAlgorithmName.SHA512.Name, 1000);               // Noncompliant
        var pdb6 = new PasswordDeriveBytes(passwordString, constantSalt, HashAlgorithmName.SHA512.Name, 1000);              // Noncompliant
        var pdb7 = new PasswordDeriveBytes(passwordBytes, constantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams);    // Noncompliant
        var pdb8 = new PasswordDeriveBytes(passwordString, constantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams);   // Noncompliant

        var pbkdf2a = new Rfc2898DeriveBytes(passwordString, constantSalt);                                                 // Noncompliant {{Make this salt unpredictable.}}
        var pbkdf2b = new Rfc2898DeriveBytes(passwordString, constantSalt, 1000);                                           // Noncompliant
        var pbkdf2c = new Rfc2898DeriveBytes(passwordBytes, constantSalt, 1000);                                            // Noncompliant
        var pbkdf2d = new Rfc2898DeriveBytes(passwordString, constantSalt, 1000, HashAlgorithmName.SHA512);                 // Noncompliant
    }

    public void RNGCryptoServiceProviderIsCompliant()
    {
        var getBytesSalt = new byte[16];

        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(getBytesSalt);
            var pdb1 = new PasswordDeriveBytes(passwordBytes, getBytesSalt);
            var pbkdf1 = new Rfc2898DeriveBytes(passwordString, getBytesSalt);

            var getNonZeroBytesSalt = new byte[16];
            rng.GetNonZeroBytes(getNonZeroBytesSalt);
            var pdb2 = new PasswordDeriveBytes(passwordBytes, getBytesSalt);
            var pbkdf2 = new Rfc2898DeriveBytes(passwordString, getBytesSalt);

            var shortSalt = new byte[15];
            rng.GetBytes(shortSalt);
            var pdb3 = new PasswordDeriveBytes(passwordBytes, shortSalt);   // Noncompliant {{Make this salt at least 16 bytes.}}
            var pbkdf3 = new Rfc2898DeriveBytes(passwordString, shortSalt); // Noncompliant
        }
    }

    public void RandomNumberGeneratorIsCompliant()
    {
        var getBytesSalt = new byte[16];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(getBytesSalt);
            var pdb1 = new PasswordDeriveBytes(passwordBytes, getBytesSalt);
            var pbkdf1 = new Rfc2898DeriveBytes(passwordString, getBytesSalt);

            var getNonZeroBytesSalt = new byte[16];
            rng.GetNonZeroBytes(getNonZeroBytesSalt);
            var pdb2 = new PasswordDeriveBytes(passwordBytes, getBytesSalt);
            var pbkdf2 = new Rfc2898DeriveBytes(passwordString, getBytesSalt);

            var shortSalt = new byte[15];
            rng.GetBytes(shortSalt);
            var pdb3 = new PasswordDeriveBytes(passwordBytes, shortSalt);   // Noncompliant {{Make this salt at least 16 bytes.}}
            var pbkdf3 = new Rfc2898DeriveBytes(passwordString, shortSalt); // Noncompliant
        }
    }

    // System.Random generates pseudo-random numbers, therefore it's not suitable to generate crypthoraphically secure random numbers.
    public void SystemRandomIsNotCompliant()
    {
        var rnd = new Random();
        var saltCustom = new byte[16];
        for (int i = 0; i < saltCustom.Length; i++)
        {
            saltCustom[i] = (byte)rnd.Next(255);
        }
        new PasswordDeriveBytes(passwordBytes, saltCustom);         // Noncompliant
    }

    public void SaltAsParameter(byte[] salt)
    {
        var pdb = new PasswordDeriveBytes(passwordBytes, salt);     // Compliant, we know nothing about salt
        var pbkdf = new Rfc2898DeriveBytes(passwordString, salt);   // Compliant, we know nothing about salt
    }

    public void ToArrayInvocation()
    {
        var noncompliantSalt = new byte[15];
        var pdb1 = new PasswordDeriveBytes(passwordBytes, noncompliantSalt.ToArray());              // FN
        var pdb2 = new PasswordDeriveBytes(passwordBytes, noncompliantSalt.ToList().ToArray());     // FN

        var compliantSalt = new byte[16];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(compliantSalt);
        var pdb3 = new PasswordDeriveBytes(passwordBytes, compliantSalt.ToArray());                 // Compliant
    }

    public void EncodingGetBytesWithStringLiterals(string saltAsText)
    {
        var constantSalt1 = Encoding.UTF8.GetBytes("HardcodedText");
        var constantSalt2 = Encoding.Unicode.GetBytes((string)"HardcodedText");
        var pdb1 = new PasswordDeriveBytes(passwordBytes, constantSalt1);   // Noncompliant
        var pdb2 = new PasswordDeriveBytes(passwordBytes, constantSalt2);   // Noncompliant

        var constantSalt3 = new byte[16];
        Encoding.UTF8.GetBytes("HardcodedText", 0, 1, constantSalt3, 0);
        var pdb3 = new PasswordDeriveBytes(passwordBytes, constantSalt3);   // Noncompliant

        string hardcodedTextInLocalVariable = "HardcodedText";
        var constantSalt4 = Encoding.UTF8.GetBytes(hardcodedTextInLocalVariable);
        var pdb4 = new PasswordDeriveBytes(passwordBytes, constantSalt4);   // FN

        const string constantText = "HardcodedText";
        var constantSalt5 = Encoding.UTF8.GetBytes(constantText);
        var pdb5 = new PasswordDeriveBytes(passwordBytes, constantSalt5);   // FN

        var notConstantSalt = Encoding.UTF8.GetBytes(saltAsText);
        var pdb6 = new PasswordDeriveBytes(passwordBytes, notConstantSalt); // Compliant - we don't know where the argument is coming from
    }

    public void ImplicitSaltIsCompliant(string password)
    {
        var withAutomaticSalt1 = new Rfc2898DeriveBytes(passwordString, saltSize: 16);
        var withAutomaticSalt2 = new Rfc2898DeriveBytes(passwordString, 16, 1000);
        var withAutomaticSalt3 = new Rfc2898DeriveBytes(passwordString, 16, 1000, HashAlgorithmName.SHA512);

        var withAutomaticSalt4 = new Rfc2898DeriveBytes(passwordString, saltSize: 16);
        var withAutomaticSalt5 = new Rfc2898DeriveBytes(passwordString, 16, 1000);
        var withAutomaticSalt6 = new Rfc2898DeriveBytes(passwordString, 16, 1000, HashAlgorithmName.SHA512);
    }

    public void Conditional(int arg)
    {
        var rng = RandomNumberGenerator.Create();
        var salt = new byte[16];
        if (arg == 1)
        {
            rng.GetBytes(salt);
            var pdb1 = new PasswordDeriveBytes(passwordBytes, salt);                                // Compliant
        }
        var pdb2 = new PasswordDeriveBytes(passwordBytes, salt);                                    // Noncompliant {{Make this salt unpredictable.}}

        var noncompliantSalt = new byte[16];
        var compliantSalt = new byte[16];
        rng.GetBytes(compliantSalt);
        var salt3 = arg == 2 ? compliantSalt : noncompliantSalt;
        var pdb3 = new PasswordDeriveBytes(passwordBytes, salt3);                                   // Noncompliant

        var salt4 = true ? compliantSalt : noncompliantSalt;
        var pdb4 = new PasswordDeriveBytes(passwordBytes, salt4);                                   // Compliant

        var pdb5 = new PasswordDeriveBytes(passwordBytes, true ? new byte[16] : compliantSalt);     // Noncompliant
        var pdb6 = new PasswordDeriveBytes(passwordBytes, true ? compliantSalt : new byte[16]);     // Compliant
    }

    public void TryCatchFinally()
    {
        var salt = new byte[16];
        try
        {
            var pdb1 = new PasswordDeriveBytes(passwordBytes, salt);                                // Noncompliant
        }
        catch
        {
            var pdb2 = new PasswordDeriveBytes(passwordBytes, salt);                                // Noncompliant
        }
        finally
        {
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
        }
        var pdb3 = new PasswordDeriveBytes(passwordBytes, salt);                                    // Compliant
    }

    public void AssignedToAnotherVariable()
    {
        new PasswordDeriveBytes(passwordBytes, new byte[16]);                                       // Noncompliant
    }

    public void Lambda()
    {
        Action<byte[]> a = (byte[] passwordBytes) =>
        {
            var shortSalt = new byte[15];
            new PasswordDeriveBytes(passwordBytes, shortSalt);                                      // Noncompliant
        };
    }

    public void AliasedTypeAndFullName()
    {
        var shortAndConstantSalt = new byte[15];
        new AliasedPasswordDeriveBytes(passwordBytes, shortAndConstantSalt);                        // Noncompliant
        new System.Security.Cryptography.PasswordDeriveBytes(passwordBytes, shortAndConstantSalt);  // Noncompliant
    }

    public void InnerMethod()
    {
        Inner();

        void Inner()
        {
            var shortSalt = new byte[15];
            new PasswordDeriveBytes(passwordBytes, shortSalt);                                      // Noncompliant
        }
    }

    public void ByteArrayCases(byte[] passwordBytes)
    {
        var rng = RandomNumberGenerator.Create();

        var multiDimensional = new byte[1][];
        rng.GetBytes(multiDimensional[0]);
        new PasswordDeriveBytes(passwordBytes, multiDimensional[0]);                                // FN, not supported

        var shortArray = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
        rng.GetBytes(shortArray);
        new PasswordDeriveBytes(passwordBytes, shortArray);                                         // Noncompliant

        var longEnoughArray = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        rng.GetBytes(longEnoughArray);
        new PasswordDeriveBytes(passwordBytes, longEnoughArray);                                    // Compliant

        var evenLongerArray = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        rng.GetBytes(evenLongerArray);
        new PasswordDeriveBytes(passwordBytes, evenLongerArray);                                    // Compliant

        var longButNotRandomArray = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        new PasswordDeriveBytes(passwordBytes, longButNotRandomArray);                              // Noncompliant

        new PasswordDeriveBytes(passwordBytes, GetSalt());                                          // Compliant

        var returnedByMethod = GetSalt();
        new PasswordDeriveBytes(passwordBytes, returnedByMethod);                                   // Compliant
    }

    public void UsingCustomPasswordDeriveClass()
    {
        var salt = new byte[16];
        var pdb1 = new CustomPasswordDeriveClass("somepassword", salt);                             // Noncompliant

        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        var pdb2 = new CustomPasswordDeriveClass("somepassword", salt);                             // Compliant
    }

    private byte[] GetSalt() => null;

    private class CustomPasswordDeriveClass : DeriveBytes
    {
        public CustomPasswordDeriveClass(string password, byte[] salt)
        {
        }

        public override byte[] GetBytes(int cb) => null;

        public override void Reset()
        {
        }
    }
}

public class FieldsAndConstants
{
    private const int UnsafeSaltSize = 15;
    private const int SafeSaltSize = 16;
    private byte[] saltField = new byte[16]; // Salt as field is not tracked by the SE engine

    public void SaltStoredInField(byte[] passwordBytes)
    {
        new PasswordDeriveBytes(passwordBytes, saltField);              // FN
        new Rfc2898DeriveBytes(passwordBytes, saltField, 16);           // Compliant

        saltField = new byte[16];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltField);
        new PasswordDeriveBytes(passwordBytes, saltField);              // Compliant

        saltField = new byte[15];
        new PasswordDeriveBytes(passwordBytes, saltField);              // Noncompliant
    }

    public void SaltSizeFromConstantField(byte[] passwordBytes)
    {
        var rng = RandomNumberGenerator.Create();
        var unsafeSalt = new byte[UnsafeSaltSize];
        rng.GetBytes(unsafeSalt);
        new PasswordDeriveBytes(passwordBytes, unsafeSalt);             // Noncompliant

        var safeSalt = new byte[SafeSaltSize];
        rng.GetBytes(safeSalt);
        new PasswordDeriveBytes(passwordBytes, safeSalt);               // Compliant
    }
}
