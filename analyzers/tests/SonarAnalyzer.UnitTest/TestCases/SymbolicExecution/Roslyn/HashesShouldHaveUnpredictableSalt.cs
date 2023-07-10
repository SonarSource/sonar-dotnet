using System;
using System.Security.Cryptography;
using System.Text;

namespace Tests.Diagnostics
{
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
            var pdb1 = new PasswordDeriveBytes(passwordBytes, shortAndConstantSalt); // FIXME Non-compliant {{Make this salt unpredictable.}}
            //                                                ^^^^^^^^^^^^^^^^^^^^
            var pdb2 = new PasswordDeriveBytes(passwordString, shortAndConstantSalt); // FIXME Non-compliant {{Make this salt unpredictable.}}
            var pdb3 = new PasswordDeriveBytes(passwordBytes, shortAndConstantSalt, cspParams); // FIXME Non-compliant {{Make this salt unpredictable.}}
            var pdb4 = new PasswordDeriveBytes(passwordString, shortAndConstantSalt, cspParams); // FIXME Non-compliant {{Make this salt unpredictable.}}
            var pdb5 = new PasswordDeriveBytes(passwordBytes, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000); // FIXME Non-compliant {{Make this salt unpredictable.}}
            //                                                ^^^^^^^^^^^^^^^^^^^^
            var pdb6 = new PasswordDeriveBytes(passwordString, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000); // FIXME Non-compliant {{Make this salt unpredictable.}}
            var pdb7 = new PasswordDeriveBytes(passwordBytes, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams); // FIXME Non-compliant {{Make this salt unpredictable.}}
            var pdb8 = new PasswordDeriveBytes(passwordString, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams); // FIXME Non-compliant {{Make this salt unpredictable.}}

            var pbkdf2a = new Rfc2898DeriveBytes(passwordString, shortAndConstantSalt); // FIXME Non-compliant
            var pbkdf2b = new Rfc2898DeriveBytes(passwordString, shortAndConstantSalt, 1000); // FIXME Non-compliant
            var pbkdf2c = new Rfc2898DeriveBytes(passwordBytes, shortAndConstantSalt, 1000); // FIXME Non-compliant
            var pbkdf2d = new Rfc2898DeriveBytes(passwordString, shortAndConstantSalt, 1000, HashAlgorithmName.SHA512); // FIXME Non-compliant
        }

        public void ConstantHashIsNotCompliant()
        {
            var constantSalt = new byte[16];
            var pdb1 = new PasswordDeriveBytes(passwordBytes, constantSalt); // FIXME Non-compliant {{Make this salt unpredictable.}}
            var pdb2 = new PasswordDeriveBytes(passwordString, constantSalt); // FIXME Non-compliant
            var pdb3 = new PasswordDeriveBytes(passwordBytes, constantSalt, cspParams); // FIXME Non-compliant
            var pdb4 = new PasswordDeriveBytes(passwordString, constantSalt, cspParams); // FIXME Non-compliant
            var pdb5 = new PasswordDeriveBytes(passwordBytes, constantSalt, HashAlgorithmName.SHA512.Name, 1000); // FIXME Non-compliant
            var pdb6 = new PasswordDeriveBytes(passwordString, constantSalt, HashAlgorithmName.SHA512.Name, 1000); // FIXME Non-compliant
            var pdb7 = new PasswordDeriveBytes(passwordBytes, constantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams); // FIXME Non-compliant
            var pdb8 = new PasswordDeriveBytes(passwordString, constantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams); // FIXME Non-compliant

            var pbkdf2a = new Rfc2898DeriveBytes(passwordString, constantSalt); // FIXME Non-compliant {{Make this salt unpredictable.}}
            var pbkdf2b = new Rfc2898DeriveBytes(passwordString, constantSalt, 1000); // FIXME Non-compliant
            var pbkdf2c = new Rfc2898DeriveBytes(passwordBytes, constantSalt, 1000); // FIXME Non-compliant
            var pbkdf2d = new Rfc2898DeriveBytes(passwordString, constantSalt, 1000, HashAlgorithmName.SHA512); // FIXME Non-compliant
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
                var pdb3 = new PasswordDeriveBytes(passwordBytes, shortSalt); // FIXME Non-compliant {{Make this salt at least 16 bytes.}}
                var pbkdf3 = new Rfc2898DeriveBytes(passwordString, shortSalt); // FIXME Non-compliant
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
                var pdb3 = new PasswordDeriveBytes(passwordBytes, shortSalt); // FIXME Non-compliant {{Make this salt at least 16 bytes.}}
                var pbkdf3 = new Rfc2898DeriveBytes(passwordString, shortSalt); // FIXME Non-compliant
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
            new PasswordDeriveBytes(passwordBytes, saltCustom); // FIXME Non-compliant
        }

        public void SaltAsParameter(byte[] salt)
        {
            var pdb = new PasswordDeriveBytes(passwordBytes, salt); // Compliant, we know nothing about salt
            var pbkdf = new Rfc2898DeriveBytes(passwordString, salt); // Compliant, we know nothing about salt
        }

        public void SaltWithEncodingGetBytes(string value)
        {
            var salt = Encoding.UTF8.GetBytes(value);
            var pdb = new PasswordDeriveBytes(passwordString, salt); // Compliant, we don't know how the salt was created
            var rfcPdb = new Rfc2898DeriveBytes(passwordString, salt); // Compliant
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

        public void Conditional(int arg, string password)
        {
            var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            if (arg == 1)
            {
                rng.GetBytes(salt);
                new PasswordDeriveBytes(passwordBytes, salt); // Compliant
            }
            new PasswordDeriveBytes(passwordBytes, salt); // FIXME Non-compliant {{Make this salt unpredictable.}}

            var noncompliantSalt = new byte[16];
            var compliantSalt = new byte[16];
            var salt3 = arg == 2 ? compliantSalt : noncompliantSalt;
            new PasswordDeriveBytes(passwordBytes, salt3); // FIXME Non-compliant
        }

        public void AssignedToAnotherVariable()
        {
            var noncompliantSalt = new byte[16];
            var compliantSalt = new byte[16];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(compliantSalt);

            var salt1 = compliantSalt;
            new PasswordDeriveBytes(passwordBytes, salt1);

            var salt2 = noncompliantSalt;
            new PasswordDeriveBytes(passwordBytes, salt2); // FIXME Non-compliant

            noncompliantSalt = compliantSalt;
            new PasswordDeriveBytes(passwordBytes, noncompliantSalt);

            new PasswordDeriveBytes(passwordBytes, new byte[16]); // FIXME Non-compliant
        }

        public void Lambda()
        {
            Action<byte[]> a = (byte[] passwordBytes) =>
            {
                var shortSalt = new byte[15];
                new PasswordDeriveBytes(passwordBytes, shortSalt); // FIXME Non-compliant
            };
        }

        public void InnerMethod()
        {
            Inner();

            void Inner()
            {
                var shortSalt = new byte[15];
                new PasswordDeriveBytes(passwordBytes, shortSalt); // FIXME Non-compliant
            }
        }

        public void ByteArrayCases(byte[] passwordBytes)
        {
            var rng = RandomNumberGenerator.Create();

            var multiDimensional = new byte[1][];
            rng.GetBytes(multiDimensional[0]);
            new PasswordDeriveBytes(passwordBytes, multiDimensional[0]); // FN, not supported

            var shortArray = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            rng.GetBytes(shortArray);
            new PasswordDeriveBytes(passwordBytes, shortArray); // FIXME Non-compliant

            var longEnoughArray = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            rng.GetBytes(longEnoughArray);
            new PasswordDeriveBytes(passwordBytes, longEnoughArray); // Compliant

            new PasswordDeriveBytes(passwordBytes, GetSalt()); // Compliant

            var returnedByMethod = GetSalt();
            new PasswordDeriveBytes(passwordBytes, returnedByMethod); // Compliant
        }

        private byte[] GetSalt() => null;
    }

    public class FieldsAndConstants
    {
        private const int UnsafeSaltSize = 15;
        private const int SafeSaltSize = 16;
        private byte[] saltField = new byte[16]; // Salt as field is not tracked by the SE engine

        public void SaltStoredInField(byte[] passwordBytes)
        {
            new PasswordDeriveBytes(passwordBytes, saltField); // Compliant
            new Rfc2898DeriveBytes(passwordBytes, saltField, 16); // Compliant

            saltField = new byte[16];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltField);
            new PasswordDeriveBytes(passwordBytes, saltField); // Compliant

            saltField = new byte[15];
            new PasswordDeriveBytes(passwordBytes, saltField); // FIXME Non-compliant

            var unsafeSalt = new byte[UnsafeSaltSize];
            rng.GetBytes(unsafeSalt);
            new PasswordDeriveBytes(passwordBytes, unsafeSalt); // FIXME Non-compliant

            var safeSalt = new byte[SafeSaltSize];
            rng.GetBytes(safeSalt);
            new PasswordDeriveBytes(passwordBytes, safeSalt); // Compliant
        }

        public void SaltSizeFromConstantField(byte[] passwordBytes)
        {
            var unsafeSalt = new byte[UnsafeSaltSize];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(unsafeSalt);
            new PasswordDeriveBytes(passwordBytes, unsafeSalt); // FIXME Non-compliant

            var safeSalt = new byte[SafeSaltSize];
            rng.GetBytes(safeSalt);
            new PasswordDeriveBytes(passwordBytes, safeSalt); // Compliant
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7355
public class AD0001_Repro
{
    // This reproduces scenario when S1944 check is not active as a rule, but still present in the exploded graph as an engine check.
    // It must be part of another rule to reproduce the original problem
    public void InvalidCastToInterfaceSymbolicExecution_Repro()
    {
        int? i = null;
        int j = (int)i; // Compliant in this rule, would raise S1944 in the old SE
    }
}
