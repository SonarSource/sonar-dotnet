using System.Security.Cryptography;
using System.Text;

namespace Tests.Diagnostics
{
    class Program
    {
        private const string passwordString = "Secret";
        private CspParameters cspParams = new CspParameters();
        private readonly byte[] passwordBytes = Encoding.UTF8.GetBytes(passwordString);

        public void ShortHashIsNotCompliant()
        {
            var shortSalt = new byte[31];
            var pdb1 = new PasswordDeriveBytes(passwordBytes, shortSalt); // Noncompliant {{Make this salt longer.}}
//                                                            ^^^^^^^^^
            var pdb2 = new PasswordDeriveBytes(passwordString, shortSalt); // Noncompliant {{Make this salt longer.}}
            var pdb3 = new PasswordDeriveBytes(passwordBytes, shortSalt, cspParams); // Noncompliant {{Make this salt longer.}}
            var pdb4 = new PasswordDeriveBytes(passwordString, shortSalt, cspParams); // Noncompliant {{Make this salt longer.}}
            var pdb5 = new PasswordDeriveBytes(passwordBytes, shortSalt, HashAlgorithmName.SHA512.Name, 1000); // Noncompliant {{Make this salt longer.}}
//                                                            ^^^^^^^^^
            var pdb6 = new PasswordDeriveBytes(passwordString, shortSalt, HashAlgorithmName.SHA512.Name, 1000); // Noncompliant {{Make this salt longer.}}
            var pdb7 = new PasswordDeriveBytes(passwordBytes, shortSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams); // Noncompliant {{Make this salt longer.}}
            var pdb8 = new PasswordDeriveBytes(passwordString, shortSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams); // Noncompliant {{Make this salt longer.}}

            var pbkdf2a = new Rfc2898DeriveBytes(passwordString, shortSalt); // Noncompliant
            var pbkdf2b = new Rfc2898DeriveBytes(passwordString, shortSalt, 1000); // Noncompliant
            var pbkdf2c = new Rfc2898DeriveBytes(passwordBytes, shortSalt, 1000); // Noncompliant
            var pbkdf2d = new Rfc2898DeriveBytes(passwordString, shortSalt, 1000, HashAlgorithmName.SHA512); // Noncompliant
        }

        public void ConstantHashIsNotCompliant()
        {
            var constantSalt = new byte[32];
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
            var getBytesSalt = new byte[32];

            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(getBytesSalt);
            var pdb1 = new PasswordDeriveBytes(passwordBytes, getBytesSalt);
            var pbkdf1 = new Rfc2898DeriveBytes(passwordString, getBytesSalt);

            var getNonZeroBytesSalt = new byte[32];
            rng.GetNonZeroBytes(getNonZeroBytesSalt);
            var pdb2 = new PasswordDeriveBytes(passwordBytes, getBytesSalt);
            var pbkdf2 = new Rfc2898DeriveBytes(passwordString, getBytesSalt);

            var shortHash = new byte[31];
            rng.GetBytes(shortHash);
            var pdb3 = new PasswordDeriveBytes(passwordBytes, shortHash); // Noncompliant
            var pbkdf3 = new Rfc2898DeriveBytes(passwordString, shortHash); // Noncompliant
        }
    }
}
