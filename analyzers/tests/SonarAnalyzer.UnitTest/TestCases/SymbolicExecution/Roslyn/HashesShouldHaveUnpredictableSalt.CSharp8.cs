using System.Security.Cryptography;

public class Sample
{
    public void Conditional(int arg, string password, byte[] passwordBytes)
    {
        var salt = new byte[16];

        DeriveBytes e = arg switch
        {
            1 => new Rfc2898DeriveBytes(password, salt),        // FIXME Non-compliant
            2 => new PasswordDeriveBytes(passwordBytes, salt),  // FIXME Non-compliant
            _ => null
        };
    }
}

public interface InterfaceWithMethodImplementation
{
    public void Method()
    {
        var salt = new byte[16];
        new Rfc2898DeriveBytes(password, salt),                 // FIXME Non-compliant
    }
}
