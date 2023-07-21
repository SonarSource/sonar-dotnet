Imports System.Security.Cryptography

' The Rfc2898DeriveBytes.Pbkdf2 method is available in .NET 6.0+
Public Class Pbkdf2Method
    Public Sub DeriveKeyWithPbkdf2Method(passwordBytes As Byte(), passwordText As String)
        Dim rng = RandomNumberGenerator.Create()
        Dim unsafeSalt = New Byte(14) {}
        Dim safeSalt = New Byte(15) {}
        rng.GetBytes(safeSalt)

        Dim unsafeSpan = unsafeSalt.AsSpan()
        Dim safeSpan = safeSalt.AsSpan()

        Dim key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, unsafeSalt, 42, HashAlgorithmName.SHA512, 42)                        ' Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordText, unsafeSalt, 42, HashAlgorithmName.SHA512, 42)                             ' Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordText.AsSpan(), unsafeSalt, 42, HashAlgorithmName.SHA512, 42)                    ' Noncompliant

        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, unsafeSpan, 42, HashAlgorithmName.SHA512, 42)                            ' Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, unsafeSalt.AsSpan(), 42, HashAlgorithmName.SHA512, 42)                   ' Noncompliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes.AsSpan(), unsafeSpan, 42, HashAlgorithmName.SHA512, 42)                   ' Noncompliant

        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, safeSalt, 42, HashAlgorithmName.SHA512, 42)                              ' Compliant
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, safeSpan, 42, HashAlgorithmName.SHA512, 42)
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, safeSalt.AsSpan(), 42, HashAlgorithmName.SHA512, 42)
        key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes.AsSpan(), safeSpan, 42, HashAlgorithmName.SHA512, 42)
    End Sub
End Class
