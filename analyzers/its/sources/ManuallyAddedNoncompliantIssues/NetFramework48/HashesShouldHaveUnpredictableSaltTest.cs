using System.Security.Cryptography;

namespace NetFramework48
{
    public class HashesShouldHaveUnpredictableSaltTest
    {
        public void TestCases(byte[] passwordBytes)
        {
            var rng = RandomNumberGenerator.Create();

            var shortSalt = new byte[15];
            new PasswordDeriveBytes(passwordBytes, shortSalt); // Noncompliant (S2053) {{Make this salt unpredictable.}}
            new Rfc2898DeriveBytes(passwordBytes, shortSalt, 1); // Noncompliant

            var safeSalt = new byte[16];
            rng.GetBytes(safeSalt);
            new PasswordDeriveBytes(passwordBytes, safeSalt);
            new Rfc2898DeriveBytes(passwordBytes, safeSalt, 1);
        }
    }
}
