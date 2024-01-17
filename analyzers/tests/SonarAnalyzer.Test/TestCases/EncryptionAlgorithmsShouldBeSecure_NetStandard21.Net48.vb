Imports System
Imports System.Security.Cryptography

Namespace Tests.Diagnostics
    Class RSAEncryptionPaddingTest
        Private rsaProvider As RSACryptoServiceProvider = New RSACryptoServiceProvider()

        Private Sub InvocationSetsAllowedValue(ByVal data As Byte(), ByVal padding As RSAEncryptionPadding, ByVal genericRSA As RSA)
            Dim bytesWritten = Nothing
            rsaProvider.TryEncrypt(data, Nothing, RSAEncryptionPadding.OaepSHA256, bytesWritten)

            genericRSA.Encrypt(data, RSAEncryptionPadding.OaepSHA256)
            genericRSA.TryEncrypt(data, Nothing, RSAEncryptionPadding.OaepSHA256, bytesWritten)
        End Sub

        Private Sub InvocationSetsNotAllowedValue(ByVal data As Byte(), ByVal genericRSA As RSA)
            Dim bytesWritten = Nothing
            rsaProvider.TryEncrypt(data, Nothing, RSAEncryptionPadding.Pkcs1, bytesWritten) ' Noncompliant

            genericRSA.Encrypt(data, RSAEncryptionPadding.Pkcs1) ' Noncompliant
            genericRSA.TryEncrypt(data, Nothing, RSAEncryptionPadding.Pkcs1, bytesWritten) ' Noncompliant
        End Sub
    End Class
End Namespace
