using System.Security.Cryptography;

public class Sample
{
    public void Conditional(int arg, string password, byte[] passwordBytes)
    {
        var salt = new byte[16];

        DeriveBytes e = arg switch
        {
            1 => new Rfc2898DeriveBytes(password, salt),        // Noncompliant
            2 => new PasswordDeriveBytes(passwordBytes, salt),  // Noncompliant
            _ => null
        };
    }
}
