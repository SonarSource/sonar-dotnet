using System.Security.Cryptography;
using System.Text;

public class Sample
{
    public void Examples()
    {
        const string passwordString = "Secret";
        var passwordBytes = Encoding.UTF8.GetBytes("Secret");

        var shortSalt = "123456789012345"u8.ToArray();

        var safeSalt = "1234567890123456"u8.ToArray();
        RandomNumberGenerator.Create().GetNonZeroBytes(safeSalt);

        using var pdb1 = new PasswordDeriveBytes(passwordBytes, shortSalt); // FN
        using var pdb2 = new PasswordDeriveBytes(passwordBytes, safeSalt);

        new Rfc2898DeriveBytes(passwordString, shortSalt);                  // FN
        new Rfc2898DeriveBytes(passwordString, safeSalt);
    }
}
