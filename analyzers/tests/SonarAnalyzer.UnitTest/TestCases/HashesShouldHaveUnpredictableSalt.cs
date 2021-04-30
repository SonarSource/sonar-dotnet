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

        public void ShortSaltIsNotCompliant()
        {
            var shortSalt = new byte[15];
            var pdb1 = new PasswordDeriveBytes(passwordBytes, shortSalt); // Noncompliant {{Make this salt unpredictable.}}
//                                                            ^^^^^^^^^
            var pdb2 = new PasswordDeriveBytes(passwordString, shortSalt); // Noncompliant {{Make this salt unpredictable.}}
            var pdb3 = new PasswordDeriveBytes(passwordBytes, shortSalt, cspParams); // Noncompliant {{Make this salt unpredictable.}}
            var pdb4 = new PasswordDeriveBytes(passwordString, shortSalt, cspParams); // Noncompliant {{Make this salt unpredictable.}}
            var pdb5 = new PasswordDeriveBytes(passwordBytes, shortSalt, HashAlgorithmName.SHA512.Name, 1000); // Noncompliant {{Make this salt unpredictable.}}
//                                                            ^^^^^^^^^
        var pdb6 = new PasswordDeriveBytes(passwordString, shortSalt, HashAlgorithmName.SHA512.Name, 1000); // Noncompliant {{Make this salt unpredictable.}}
            var pdb7 = new PasswordDeriveBytes(passwordBytes, shortSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams); // Noncompliant {{Make this salt unpredictable.}}
            var pdb8 = new PasswordDeriveBytes(passwordString, shortSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams); // Noncompliant {{Make this salt unpredictable.}}

            var pbkdf2a = new Rfc2898DeriveBytes(passwordString, shortSalt); // Noncompliant
            var pbkdf2b = new Rfc2898DeriveBytes(passwordString, shortSalt, 1000); // Noncompliant
            var pbkdf2c = new Rfc2898DeriveBytes(passwordBytes, shortSalt, 1000); // Noncompliant
            var pbkdf2d = new Rfc2898DeriveBytes(passwordString, shortSalt, 1000, HashAlgorithmName.SHA512); // Noncompliant
        }

        public void ConstantHashIsNotCompliant()
        {
            var constantSalt = new byte[16];
            var pdb1 = new PasswordDeriveBytes(passwordBytes, constantSalt); // Noncompliant {{Make this salt unpredictable.}}
            var pdb2 = new PasswordDeriveBytes(passwordString, constantSalt); // Noncompliant
            var pdb3 = new PasswordDeriveBytes(passwordBytes, constantSalt, cspParams); // Noncompliant
            var pdb4 = new PasswordDeriveBytes(passwordString, constantSalt, cspParams); // Noncompliant
            var pdb5 = new PasswordDeriveBytes(passwordBytes, constantSalt, HashAlgorithmName.SHA512.Name, 1000); // Noncompliant
            var pdb6 = new PasswordDeriveBytes(passwordString, constantSalt, HashAlgorithmName.SHA512.Name, 1000); // Noncompliant
            var pdb7 = new PasswordDeriveBytes(passwordBytes, constantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams); // Noncompliant
            var pdb8 = new PasswordDeriveBytes(passwordString, constantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams); // Noncompliant

            var pbkdf2a = new Rfc2898DeriveBytes(passwordString, constantSalt); // Noncompliant {{Make this salt unpredictable.}}
            var pbkdf2b = new Rfc2898DeriveBytes(passwordString, constantSalt, 1000); // Noncompliant
            var pbkdf2c = new Rfc2898DeriveBytes(passwordBytes, constantSalt, 1000); // Noncompliant
            var pbkdf2d = new Rfc2898DeriveBytes(passwordString, constantSalt, 1000, HashAlgorithmName.SHA512); // Noncompliant
        }

        public void RNGCryptoServiceProviderIsCompliant()
        {
            var getBytesSalt = new byte[16];

            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(getBytesSalt);
            var pdb1 = new PasswordDeriveBytes(passwordBytes, getBytesSalt);
            var pbkdf1 = new Rfc2898DeriveBytes(passwordString, getBytesSalt);

            var getNonZeroBytesSalt = new byte[16];
            rng.GetNonZeroBytes(getNonZeroBytesSalt);
            var pdb2 = new PasswordDeriveBytes(passwordBytes, getBytesSalt);
            var pbkdf2 = new Rfc2898DeriveBytes(passwordString, getBytesSalt);

            var shortSalt = new byte[15];
            rng.GetBytes(shortSalt);
            var pdb3 = new PasswordDeriveBytes(passwordBytes, shortSalt); // Noncompliant
            var pbkdf3 = new Rfc2898DeriveBytes(passwordString, shortSalt); // Noncompliant
        }

        public void RandomNumberGeneratorIsCompliant()
        {
            var getBytesSalt = new byte[16];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(getBytesSalt);
            var pdb1 = new PasswordDeriveBytes(passwordBytes, getBytesSalt);
            var pbkdf1 = new Rfc2898DeriveBytes(passwordString, getBytesSalt);

            var getNonZeroBytesSalt = new byte[16];
            rng.GetNonZeroBytes(getNonZeroBytesSalt);
            var pdb2 = new PasswordDeriveBytes(passwordBytes, getBytesSalt);
            var pbkdf2 = new Rfc2898DeriveBytes(passwordString, getBytesSalt);

            var shortSalt = new byte[15];
            rng.GetBytes(shortSalt);
            var pdb3 = new PasswordDeriveBytes(passwordBytes, shortSalt); // Noncompliant {{Make this salt at least 16 bytes.}}
            var pbkdf3 = new Rfc2898DeriveBytes(passwordString, shortSalt); // Noncompliant
        }

        public void SaltAsParameter(byte[] salt)
        {
            var pdb = new PasswordDeriveBytes(passwordBytes, salt); // Compliant, we know nothing about salt
            var pbkdf = new Rfc2898DeriveBytes(passwordString, salt); // Compliant, we know nothing about salt
        }

        public void SaltWithEncodingGetBytes(string value)
        {
            var salt = Encoding.UTF8.GetBytes(value);
            var pdb = new PasswordDeriveBytes(passwordString, salt); // Compliant, we don't know how to salt was created
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

        public void DifferentCases(int a, string password)
        {
            var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];

            DeriveBytes e = a switch
            {
                1 => new Rfc2898DeriveBytes(password, salt), // Noncompliant
                2 => new PasswordDeriveBytes(passwordBytes, salt), // Noncompliant
                _ => null
            };

            var salt2 = new byte[16];
            if (a == 1)
            {
                rng.GetBytes(salt2);
                new PasswordDeriveBytes(passwordBytes, salt2); // Compliant
            }
            new PasswordDeriveBytes(passwordBytes, salt2); // Noncompliant {{Make this salt unpredictable.}}

            var noncompliantSalt = new byte[16];
            var compliantSalt = new byte[16];
            rng.GetBytes(compliantSalt);

            var salt3 = a == 2 ? compliantSalt : noncompliantSalt;
            new PasswordDeriveBytes(passwordBytes, salt3); // Noncompliant

            var salt4 = compliantSalt;
            new PasswordDeriveBytes(passwordBytes, salt4);

            var salt5 = noncompliantSalt;
            new PasswordDeriveBytes(passwordBytes, salt5); // Noncompliant

            noncompliantSalt = compliantSalt;
            new PasswordDeriveBytes(passwordBytes, noncompliantSalt);

            new PasswordDeriveBytes(passwordBytes, new byte[16]); // Noncompliant

            var rnd = new Random();
            var saltCustom = new byte[16];
            for (int i = 0; i < saltCustom.Length; i++)
            {
                saltCustom[i] = (byte)rnd.Next(255);
            }
            new PasswordDeriveBytes(passwordBytes, saltCustom); // Noncompliant
        }

        public void ByteArrayCases(byte[] passwordBytes)
        {
            var rng = RandomNumberGenerator.Create();
            var a1 = new byte[1,1];

            var a2 = new byte[1][];
            rng.GetBytes(a2[0]);
            new PasswordDeriveBytes(passwordBytes, a2[0]); // FN, not supported

            var a3 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            rng.GetBytes(a3);
            new PasswordDeriveBytes(passwordBytes, a3); // Noncompliant

            var a4 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            rng.GetBytes(a4);
            new PasswordDeriveBytes(passwordBytes, a4); // Compliant

            new PasswordDeriveBytes(passwordBytes, GetSalt()); // Compliant

            var a5 = GetSalt();
            new PasswordDeriveBytes(passwordBytes, a5); // Compliant
        }

        private byte[] GetSalt() => new byte[1];
    }

    public class Foo
    {
        private byte[] salt = new byte[16]; // Salt as field is not tracked by the SE engine
        private const int UnsafeSaltSize = 15;
        private const int SafeSaltSize = 16;

        public void Bar(byte[] passwordBytes)
        {
            new PasswordDeriveBytes(passwordBytes, salt); // Compliant
            new Rfc2898DeriveBytes(passwordBytes, salt, 16); // Compliant

            salt = new byte[16];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            new PasswordDeriveBytes(passwordBytes, salt); // Compliant

            salt = new byte[15];
            new PasswordDeriveBytes(passwordBytes, salt); // Noncompliant

            var unsafeSalt = new byte[UnsafeSaltSize];
            rng.GetBytes(unsafeSalt);
            new PasswordDeriveBytes(passwordBytes, unsafeSalt); // Noncompliant

            var safeSalt = new byte[SafeSaltSize];
            rng.GetBytes(safeSalt);
            new PasswordDeriveBytes(passwordBytes, safeSalt); // Compliant
        }
    }
}
