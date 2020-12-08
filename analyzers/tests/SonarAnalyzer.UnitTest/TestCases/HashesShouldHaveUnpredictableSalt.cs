using System.Security.Cryptography;
using System.Text;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Hash()
        {
            var passwordString = "Secret";
            var passwordBytes = Encoding.UTF8.GetBytes(passwordString);
            var shortSalt = new byte[31];
            var cspParams = new CspParameters();

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

        public void UsingEncodingBetBytes(string password)
        {
            var salt = Encoding.UTF8.GetBytes("Hardcoded salt");
            var fromHardcoded = new Rfc2898DeriveBytes(password, salt); // Noncompliant, salt is hardcoded

            salt = Encoding.UTF8.GetBytes(password);
            var fromPassword = new Rfc2898DeriveBytes(password, salt); // Noncompliant, password should not be used as a salt as it makes it predictable

            var shortSalt = new byte[8];
            RandomNumberGenerator.Create().GetBytes(shortSalt);
            var fromShort = new Rfc2898DeriveBytes(password, shortSalt); // Noncompliant, salt is too short (should be at least 32 bytes, not 8)
        }

        public DeriveBytes Hash3(string password)
        {
            return new Rfc2898DeriveBytes(password, 64);
        }
    }
}
