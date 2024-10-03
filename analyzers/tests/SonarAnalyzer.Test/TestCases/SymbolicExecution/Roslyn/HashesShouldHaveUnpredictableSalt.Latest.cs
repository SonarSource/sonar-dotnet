using System;
using System.Security.Cryptography;
using System.Text;

const string passwordString = "Secret";
var passwordBytes = Encoding.UTF8.GetBytes("Secret");

var shortSalt = new byte[15];

var safeSalt = new byte[16];
RandomNumberGenerator.Create().GetNonZeroBytes(safeSalt);

using var pdb1 = new PasswordDeriveBytes(passwordBytes, shortSalt);     // Noncompliant
using var pdb2 = new PasswordDeriveBytes(passwordBytes, safeSalt);

new Rfc2898DeriveBytes(passwordString, shortSalt);                      // Noncompliant
new Rfc2898DeriveBytes(passwordString, safeSalt);

void TopLevelLocalFunction()
{
    new PasswordDeriveBytes(passwordBytes, shortSalt);                  // FN
    new PasswordDeriveBytes(passwordBytes, safeSalt);
}

public class Sample
{
    public void TargetTypedNew(byte[] passwordBytes)
    {
        var shortSalt = new byte[15];
        PasswordDeriveBytes aes = new(passwordBytes, shortSalt);        // Noncompliant

        var safeSalt = new byte[16];
        RandomNumberGenerator.Create().GetNonZeroBytes(safeSalt);
        PasswordDeriveBytes pdb = new(passwordBytes, safeSalt);
    }

    public void StaticLambda()
    {
        Action<byte[]> a = static (byte[] passwordBytes) =>
        {
            var shortSalt = new byte[15];
            new PasswordDeriveBytes(passwordBytes, shortSalt);          // Noncompliant

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
            new PasswordDeriveBytes(value, new byte[15]);               // Noncompliant

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
        new PasswordDeriveBytes(passwordBytes, new byte[15]);           // Noncompliant

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
        new PasswordDeriveBytes(passwordBytes, new byte[15]);           // Noncompliant

        var safeSalt = new byte[16];
        RandomNumberGenerator.Create().GetNonZeroBytes(safeSalt);
        new PasswordDeriveBytes(passwordBytes, safeSalt);
    }
}

public interface InterfaceWithMethodImplementation
{
    public void Method(string password)
    {
        var salt = new byte[16];
        new Rfc2898DeriveBytes(password, salt);                 // Noncompliant
    }
}

public class Examples
{
    public void CSharp10(byte[] passwordBytes)
    {
        (var shortSalt, int a) = (new byte[15], 42);
        PasswordDeriveBytes aes = new PasswordDeriveBytes(passwordBytes, shortSalt); // Noncompliant
    }

    public void CSharp11()
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

    // https://sonarsource.atlassian.net/browse/NET-363
    public void CSharp13_KMAK()
    {
        const string passwordString = "Secret";
        byte[] key = Encoding.UTF8.GetBytes("fixedKey"); // FN
        byte[] input = Encoding.UTF8.GetBytes(passwordString);
        byte[] mac = Kmac128.HashData(key, input, outputLength: 32);

        byte[] compliantKey = GenerateRandomKey(); // Compliant
        byte[] compliantInput = Encoding.UTF8.GetBytes(passwordString);
        byte[] compliantMac = Kmac128.HashData(compliantKey, compliantInput, outputLength: 32);

        using var kmac128 = new Kmac128(new byte[] { 0x01, 0x02, 0x03, 0x04 }); // FN
        using var kmac256 = new Kmac256(new byte[] { 0x01, 0x02, 0x03, 0x04 }); // FN
        using var kmacXof128 = new KmacXof128(new byte[] { 0x01, 0x02, 0x03, 0x04 }); // FN
        using var kmacXof256 = new KmacXof256(new byte[] { 0x01, 0x02, 0x03, 0x04 }); // FN

        var a = Kmac128.HashData(new byte[] { 0x01, 0x02, 0x03, 0x04 }, "Hello"u8.ToArray(), 2); // FN
        var b = Kmac256.HashData(new byte[] { 0x01, 0x02, 0x03, 0x04 }, "Hello"u8.ToArray(), 2); // FN
        var c = KmacXof128.HashData(new byte[] { 0x01, 0x02, 0x03, 0x04 }, "Hello"u8.ToArray(), 2); // FN
        var d = KmacXof256.HashData(new byte[] { 0x01, 0x02, 0x03, 0x04 }, "Hello"u8.ToArray(), 2); // FN

        static byte[] GenerateRandomKey()
        {
            using var rng = new RNGCryptoServiceProvider();
            byte[] key = new byte[32];
            rng.GetBytes(key);
            return key;
        }
    }
}
