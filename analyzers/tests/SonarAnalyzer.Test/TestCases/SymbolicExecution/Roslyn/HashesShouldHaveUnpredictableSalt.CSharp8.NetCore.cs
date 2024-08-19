using System.Security.Cryptography;

public interface InterfaceWithMethodImplementation
{
    public void Method(string password)
    {
        var salt = new byte[16];
        new Rfc2898DeriveBytes(password, salt);                 // Noncompliant
    }
}
