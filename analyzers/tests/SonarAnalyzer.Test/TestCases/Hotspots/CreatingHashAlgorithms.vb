Imports System.IO
Imports System.Security.Cryptography

Namespace Tests.Diagnostics

    Public Class InsecureHashAlgorithm

        Public Sub Hash(temp as Byte())
            Dim DSACng = new DSACng(10)
'                        ^^^^^^^^^^^^^^ {{Make sure this weak hash algorithm is not used in a sensitive context here.}}
            Dim DSACryptoServiceProvider = new DSACryptoServiceProvider() ' Noncompliant
            Dim DSACreate = DSA.Create() ' Noncompliant
            Dim DSACreateWithParam = DSA.Create("DSA") ' Noncompliant
            Dim DSACreateFromName = CryptoConfig.CreateFromName("DSA") ' Noncompliant
            Dim DSAAsymmetricAlgorithm = AsymmetricAlgorithm.Create("DSA") ' Noncompliant
            Dim DSAAsymmetricAlgorithmWithNamespace = AsymmetricAlgorithm.Create("System.Security.Cryptography.DSA") ' Noncompliant

            Dim HMACCreate = HMAC.Create() ' Noncompliant
            Dim HMACCreateWithParam = HMAC.Create("HMACMD5") ' Noncompliant

            Dim HMACMD5 = new HMACMD5() ' Noncompliant
            Dim HMACMD5Create = HMACMD5.Create() ' Noncompliant
            Dim HMACMD5CreateWithParam = HMACMD5.Create("HMACMD5") ' Noncompliant
            Dim HMACMD5KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACMD5") ' Noncompliant
            Dim HMACMD5KeyedHashAlgorithmWithNamespace = KeyedHashAlgorithm.Create("System.Security.Cryptography.HMACMD5") ' Noncompliant
            Dim HMACMD5CryptoConfig = CryptoConfig.CreateFromName("HMACMD5") ' Noncompliant

            Dim HMACSHA1 = new HMACSHA1() ' Noncompliant
            Dim HMACSHA1Create = HMACMD5.Create() ' Noncompliant
            Dim HMACSHA1CreateWithParam = HMACMD5.Create("HMACSHA1") ' Noncompliant
            Dim HMACSHA1KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACSHA1") ' Noncompliant
            Dim HMACSHA1KeyedHashAlgorithmWithNamespace = KeyedHashAlgorithm.Create("System.Security.Cryptography.HMACSHA1") ' Noncompliant
            Dim HMACSHA1CryptoConfig = CryptoConfig.CreateFromName("HMACSHA1") ' Noncompliant

            Dim HMACSHA256Create = HMACSHA256.Create("HMACSHA256")
            Dim HMACSHA256KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACSHA256")
            Dim HMACSHA256KeyedHashAlgorithmWithNamespace = KeyedHashAlgorithm.Create("System.Security.Cryptography.HMACSHA256")
            Dim HMACSHA256CryptoConfig = CryptoConfig.CreateFromName("HMACSHA256")

            Dim MD5CryptoServiceProvider = new MD5CryptoServiceProvider() ' Noncompliant
            Dim MD5CryptoConfig = CryptoConfig.CreateFromName("MD5") ' Noncompliant
            Dim MD5HashAlgorithm = HashAlgorithm.Create("MD5") ' Noncompliant
            Dim MD5HashAlgorithmWithNamespace = HashAlgorithm.Create("System.Security.Cryptography.MD5") ' Noncompliant
            Dim MD5Create = MD5.Create() ' Noncompliant
            Dim MD5CreateWithParam = MD5.Create("MD5") ' Noncompliant

            Dim SHA1Managed = new SHA1Managed() ' Noncompliant
            Dim SHA1Create = SHA1.Create() ' Noncompliant
            Dim SHA1CreateWithParam = SHA1.Create("SHA1") ' Noncompliant
            Dim SHA1HashAlgorithm = HashAlgorithm.Create("SHA1") ' Noncompliant
            Dim SHA1HashAlgorithmWithNamespace = HashAlgorithm.Create("System.Security.Cryptography.SHA1") ' Noncompliant
            Dim SHA1CryptoServiceProvider = new SHA1CryptoServiceProvider() ' Noncompliant

            Dim SHA256Managed = new SHA256Managed()
            Dim SHA256ManagedHashAlgorithm = HashAlgorithm.Create("SHA256Managed")
            Dim SHA256ManagedHashAlgorithmWithNamespace = HashAlgorithm.Create("System.Security.Cryptography.SHA256Managed")
            Dim SHA256ManagedCryptoConfig = CryptoConfig.CreateFromName("SHA256Managed")

            Dim hashAlgo = HashAlgorithm.Create()

            Dim algoName = "MD5"
            Dim MD5Var = CryptoConfig.CreateFromName(algoName) ' Noncompliant

            algoName = "SHA256Managed"
            Dim SHA256ManagedVar = CryptoConfig.CreateFromName(algoName)
        End Sub

    End Class

    Public Class MyDSA
        Inherits DSA

        Public Overrides Sub ImportParameters(parameters As DSAParameters)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function CreateSignature(rgbHash() As Byte) As Byte()
            Throw New NotImplementedException()
        End Function

        Public Overrides Function ExportParameters(includePrivateParameters As Boolean) As DSAParameters
            Throw New NotImplementedException()
        End Function

        Public Overrides Function VerifySignature(rgbHash() As Byte, rgbSignature() As Byte) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Sub UseHashData()
            Dim data = New Byte(41) {}
            Using stream = New System.IO.MemoryStream(data)
                Dim a = HashData(data, 0, data.Length, HashAlgorithmName.SHA1)                             ' Noncompliant
                Dim b = HashData(stream, HashAlgorithmName.SHA1)                                           ' Noncompliant
            End Using
        End Sub

    End Class


End Namespace
