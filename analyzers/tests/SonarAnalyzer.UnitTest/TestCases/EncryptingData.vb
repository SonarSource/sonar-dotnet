Imports System
Imports System.Security.Cryptography

Namespace MyNamespaceEncryption

    ' RSPEC example https:'jira.sonarsource.com/browse/RSPEC-4938
    Public Class Class1

        Public Sub Main()

            Dim data As Byte() = {1, 1, 1}

            Dim myRSA As RSA = RSA.Create()
            Dim padding As RSAEncryptionPadding = RSAEncryptionPadding.CreateOaep(HashAlgorithmName.SHA1)

            ' Review all base RSA class' Encrypt/Decrypt calls
            myRSA.Encrypt(data, padding)
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Make sure that encrypting data is safe here.}}
            myRSA.EncryptValue(data)       ' Noncompliant
            myRSA.Decrypt(data, padding)   ' Noncompliant
            myRSA.DecryptValue(data)       ' Noncompliant

            Dim myRSAC As RSACryptoServiceProvider = New RSACryptoServiceProvider()
            ' Review the use of any TryEncrypt/TryDecrypt And specific Encrypt/Decrypt of RSA subclasses.
            myRSAC.Encrypt(data, False)    ' Noncompliant
            myRSAC.Decrypt(data, False)    ' Noncompliant

            Dim written As Integer
            ' Note: TryEncrypt/ TryDecrypt are only in .NET Core 2.1+
            '            myRSAC.TryEncrypt(data, Span<byte>.Empty, padding, out written) ' Non compliant
            '            myRSAC.TryDecrypt(data, Span<byte>.Empty, padding, out written) ' Non compliant

            Dim rgbKey As Byte() = {1, 2, 3}
            Dim rgbIV As Byte() = {4, 5, 6}
            Dim rijn = SymmetricAlgorithm.Create()

            ' Review the creation of Encryptors from any SymmetricAlgorithm instance.
            rijn.CreateEncryptor()
'           ^^^^^^^^^^^^^^^^^^^^^^  {{Make sure that encrypting data is safe here.}}
            rijn.CreateEncryptor(rgbKey, rgbIV)    ' Noncompliant
            rijn.CreateDecryptor()                 ' Noncompliant
            rijn.CreateDecryptor(rgbKey, rgbIV)    ' Noncompliant
        End Sub

    End Class

    Public Class MyAsymmetricCrypto
        Inherits System.Security.Cryptography.AsymmetricAlgorithm ' Noncompliant
        ' ...
    End Class

    Public Class MySymmetricCrypto
        Inherits System.Security.Cryptography.SymmetricAlgorithm ' Noncompliant 

        Public Overrides Sub GenerateIV()
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub GenerateKey()
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function CreateDecryptor(rgbKey() As Byte, rgbIV() As Byte) As ICryptoTransform
            Throw New NotImplementedException()
        End Function

        Public Overrides Function CreateEncryptor(rgbKey() As Byte, rgbIV() As Byte) As ICryptoTransform
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class Class2

        Public Sub AdditionalTests(data As Byte(), padding As RSAEncryptionPadding)

            Dim customAsymProvider = New MyRSA()

            ' Should raise on derived asymmetric classes
            customAsymProvider.Encrypt(data, padding)  ' Noncompliant
            customAsymProvider.EncryptValue(data)      ' Noncompliant
            customAsymProvider.Decrypt(data, padding)  ' Noncompliant
            customAsymProvider.DecryptValue(data)      ' Noncompliant

            ' Should raise on the Try* methods added in NET Core 2.1
            ' Note: this test Is cheating - we can't currently referencing the
            ' real 2.1 assemblies since the test project Is targetting an older
            ' NET Framework, so we're testing against a custom subclass
            ' to which we've added the new method names.
            customAsymProvider.TryEncrypt()        ' Noncompliant
            customAsymProvider.TryEncrypt(Nothing)    ' Noncompliant
            customAsymProvider.TryDecrypt()        ' Noncompliant
            customAsymProvider.TryDecrypt(Nothing)    ' Noncompliant

            customAsymProvider.OtherMethod()

            ' Should raise on derived symmetric classes
            Dim customSymProvider = New MyNamespaceEncryption.MySymmetricCrypto()
            customSymProvider.CreateEncryptor()    ' Noncompliant
            customSymProvider.CreateDecryptor()    ' Noncompliant
        End Sub
    End Class

    Public Class MyRSA
        Inherits System.Security.Cryptography.RSA ' Noncompliant

        ' Dummy methods with the same names as the additional methods added in Net Core 2.1.
        Public Sub TryEncrypt()
        End Sub
        Public Sub TryEncrypt(dummyMethod As String)
        End Sub

        Public Sub TryDecrypt()
        End Sub
        Public Sub TryDecrypt(dummyMethod As String)
        End Sub

        Public Sub OtherMethod()
        End Sub


        ' Abstract methods
        Public Overrides Function ExportParameters(includePrivateParameters As Boolean) As RSAParameters
            Return New RSAParameters()
        End Function

        Public Overrides Sub ImportParameters(parameters As RSAParameters)
        End Sub
    End Class
End Namespace
