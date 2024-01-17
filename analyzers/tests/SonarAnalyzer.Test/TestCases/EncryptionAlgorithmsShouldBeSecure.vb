Imports System
Imports System.Security.Cryptography

Namespace Tests.Diagnostics

    Class SecureAesCheck

        Private field1 As AesManaged = New AesManaged() ' Noncompliant
        Private Property Property1 As AesManaged = New AesManaged() ' Noncompliant

        Private Sub CtorSetsAllowedValue(ByVal key1 As Byte())
            ' none
        End Sub

        Private Sub CtorSetsNotAllowedValue()
            Dim aesManaged = New AesManaged() ' Noncompliant {{Use secure mode and padding scheme.}}
        End Sub

        Private Sub InitializerSetsAllowedValue()
            ' none
        End Sub

        Private Sub InitializerSetsNotAllowedValue()
            Dim aesManaged = New AesManaged() With {.Mode = CipherMode.CBC} ' Noncompliant
            '                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            aesManaged = New AesManaged() With {.Mode = CipherMode.CFB} ' Noncompliant
            aesManaged = New AesManaged() With {.Mode = CipherMode.CTS} ' Noncompliant
            aesManaged = New AesManaged() With {.Mode = CipherMode.ECB} ' Noncompliant
            aesManaged = New AesManaged() With {.Mode = CipherMode.OFB} ' Noncompliant
        End Sub

        Private Sub PropertySetsNotAllowedValue()
            Dim c = New AesManaged() ' Noncompliant
            c.Mode = CipherMode.CBC ' Noncompliant
            c.Mode = CipherMode.CFB ' Noncompliant
            c.Mode = CipherMode.CTS ' Noncompliant
            c.Mode = CipherMode.ECB ' Noncompliant
            c.Mode = CipherMode.OFB ' Noncompliant
        End Sub

        Private Sub PropertySetsAllowedValue(ByVal foo As Boolean)
            ' none
        End Sub

    End Class

    Class RSAEncryptionPaddingTest

        Private rsaProvider As RSACryptoServiceProvider = New RSACryptoServiceProvider()
        Private TrueField As Boolean = True
        Private FalseField As Boolean = False

        Private Sub InvocationSetsAllowedValue(ByVal data As Byte(), ByVal padding As RSAEncryptionPadding, ByVal genericRSA As RSA)
            rsaProvider.Encrypt(data, True)
            rsaProvider.Encrypt(data, TrueField)
            rsaProvider.Decrypt(data, False)
            rsaProvider.Encrypt(fOAEP:=True, rgb:=data)
            rsaProvider.Encrypt(data, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA1)
            rsaProvider.Encrypt(data, RSAEncryptionPadding.OaepSHA256)
            rsaProvider.Encrypt(padding:=padding, data:=data)

            genericRSA.Encrypt(data, RSAEncryptionPadding.OaepSHA256)

            ' Reassigned
            TrueField = False
            rsaProvider.Encrypt(data, TrueField)            ' Noncompliant
        End Sub

        Private Sub InvocationSetsNotAllowedValue(ByVal data As Byte(), ByVal genericRSA As RSA)
            rsaProvider.Encrypt(data, False)                ' Noncompliant
            rsaProvider.Encrypt(data, FalseField)           ' Noncompliant
            rsaProvider.Encrypt(fOAEP:=False, rgb:=data)    ' Noncompliant
            rsaProvider.Encrypt(data, System.Security.Cryptography.RSAEncryptionPadding.Pkcs1) ' Noncompliant
            rsaProvider.Encrypt(data, RSAEncryptionPadding.Pkcs1) ' Noncompliant

            genericRSA.Encrypt(data, RSAEncryptionPadding.Pkcs1) ' Noncompliant

            'Reassigned
            FalseField = True
            rsaProvider.Encrypt(data, FalseField)           ' Compliant
        End Sub

    End Class

End Namespace
