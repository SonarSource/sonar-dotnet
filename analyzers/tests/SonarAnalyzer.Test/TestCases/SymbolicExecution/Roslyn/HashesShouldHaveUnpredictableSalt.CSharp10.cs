using System.Security.Cryptography;

public class Sample
{
    public void Examples(byte[] passwordBytes)
    {
        int a;
        (var shortSalt, a) = (new byte[15], 42);
        PasswordDeriveBytes aes = new PasswordDeriveBytes(passwordBytes, shortSalt); // Noncompliant
    }
}
