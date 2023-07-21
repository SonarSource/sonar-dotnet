using System;
using System.Security.Cryptography;

public interface InterfaceWithMethodImplementation
{
    public void Method(string password)
    {
        var salt = new byte[16];
        new Rfc2898DeriveBytes(password, salt);                                                                                 // Noncompliant
    }
}

// The Rfc2898DeriveBytes.Pbkdf2 method is available in .NET 6.0+
public class Pbkdf2Method
{
    public void DeriveKeyWithPbkdf2Method(byte[] passwordBytes, string passwordText)
    {
        var rng = RandomNumberGenerator.Create();
        var unsafeSalt = new byte[15];
        var safeSalt = new byte[16];
        rng.GetBytes(safeSalt);

        var unsafeSpan = unsafeSalt.AsSpan();
        var unsafeReadonlySpan = new ReadOnlySpan<byte>(unsafeSalt);
        var safeSpan = safeSalt.AsSpan();
        var safeReadonlySpan = new ReadOnlySpan<byte>(safeSalt);

        var key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, unsafeSalt, 42, HashAlgorithmName.SHA512, 42);                       // Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordText, unsafeSalt, 42, HashAlgorithmName.SHA512, 42);                            // Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordText.AsSpan(), unsafeSalt, 42, HashAlgorithmName.SHA512, 42);                   // Noncompliant

        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, unsafeSpan, 42, HashAlgorithmName.SHA512, 42);                           // Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, unsafeReadonlySpan, 42, HashAlgorithmName.SHA512, 42);                   // Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, unsafeSalt.AsSpan(), 42, HashAlgorithmName.SHA512, 42);                  // Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, new Span<byte>(unsafeSalt), 42, HashAlgorithmName.SHA512, 42);           // Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, new ReadOnlySpan<byte>(unsafeSalt), 42, HashAlgorithmName.SHA512, 42);   // Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes.AsSpan(), unsafeSpan, 42, HashAlgorithmName.SHA512, 42);                  // Noncompliant

        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, safeSalt, 42, HashAlgorithmName.SHA512, 42);                             // Compliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, safeSpan, 42, HashAlgorithmName.SHA512, 42);
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, safeReadonlySpan, 42, HashAlgorithmName.SHA512, 42);
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, safeSalt.AsSpan(), 42, HashAlgorithmName.SHA512, 42);
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, new Span<byte>(safeSalt), 42, HashAlgorithmName.SHA512, 42);
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, new ReadOnlySpan<byte>(safeSalt), 42, HashAlgorithmName.SHA512, 42);
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes.AsSpan(), safeSpan, 42, HashAlgorithmName.SHA512, 42);
    }
}
