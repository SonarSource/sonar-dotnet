using System;
using System.Security.Cryptography;
using System.Text;

const string passwordString = "Secret";
var passwordBytes = Encoding.UTF8.GetBytes("Secret");

var shortSalt = new byte[15];

var safeSalt = new byte[16];
RandomNumberGenerator.Create().GetNonZeroBytes(safeSalt);

using var pdb1 = new PasswordDeriveBytes(passwordBytes, shortSalt); // FN
using var pdb2 = new PasswordDeriveBytes(passwordBytes, safeSalt);

new Rfc2898DeriveBytes(passwordString, shortSalt); // FN
new Rfc2898DeriveBytes(passwordString, safeSalt);

void TopLevelLocalFunction()
{
    new PasswordDeriveBytes(passwordBytes, shortSalt); // FN
    new PasswordDeriveBytes(passwordBytes, safeSalt);
}

public class Sample
{
    public void TargetTypedNew(byte[] passwordBytes)
    {
        var shortSalt = new byte[15];
        PasswordDeriveBytes aes = new(passwordBytes, shortSalt); // FN

        var safeSalt = new byte[16];
        RandomNumberGenerator.Create().GetNonZeroBytes(safeSalt);
        new PasswordDeriveBytes(passwordBytes, safeSalt);
    }

    public void StaticLambda()
    {
        Action<byte[]> a = static (byte[] passwordBytes) =>
        {
            var shortSalt = new byte[15];
            new PasswordDeriveBytes(passwordBytes, shortSalt); // Noncompliant

            var safeSalt = new byte[16];
            RandomNumberGenerator.Create().GetNonZeroBytes(safeSalt);
            new PasswordDeriveBytes(passwordBytes, safeSalt);
        };
    }

    public byte[] Property
    {
        get => new byte[0];
        init
        {
            new PasswordDeriveBytes(value, new byte[15]);   // Noncompliant

            var safeSalt = new byte[16];
            RandomNumberGenerator.Create().GetNonZeroBytes(safeSalt);
            new PasswordDeriveBytes(value, safeSalt);
        }
    }
}

public record Record
{
    public void Method(byte[] passwordBytes)
    {
        new PasswordDeriveBytes(passwordBytes, new byte[15]); // Noncompliant

        var safeSalt = new byte[16];
        RandomNumberGenerator.Create().GetNonZeroBytes(safeSalt);
        new PasswordDeriveBytes(passwordBytes, safeSalt);
    }
}

public partial class Partial
{
    public partial void Method(byte[] passwordBytes);
}

public partial class Partial
{
    public partial void Method(byte[] passwordBytes)
    {
        new PasswordDeriveBytes(passwordBytes, new byte[15]); // Noncompliant

        var safeSalt = new byte[16];
        RandomNumberGenerator.Create().GetNonZeroBytes(safeSalt);
        new PasswordDeriveBytes(passwordBytes, safeSalt);
    }
}
