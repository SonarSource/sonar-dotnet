Imports System.Security.Cryptography

Namespace Tests.Diagnostics

    Public Class InsecureHashAlgorithm

        Public Sub Hash(temp as Byte())
            ' Review all instantiations of classes that inherit from HashAlgorithm, for example:
            Dim sha1 As HashAlgorithm = new SHA1Managed()
'                                       ^^^^^^^^^^^^^^^^^ {{Make sure this weak hash algorithm is not used in a sensitive context here.}}
            Dim sha1Provider As HashAlgorithm = new SHA1CryptoServiceProvider() ' Noncompliant
            Dim sha1csharp8 = new SHA1Managed() ' Noncompliant

            Dim sha256 = new SHA256Managed()
            Dim sha256Config = CryptoConfig.CreateFromName("SHA256Managed")
            Dim sha256HashAlgo = HashAlgorithm.Create("SHA256Managed")
        End Sub

        Public Sub Md5Calls(temp as Byte())
            Dim md5 = new MD5CryptoServiceProvider() ' Noncompliant
            Dim md5CryptoConfig = CryptoConfig.CreateFromName("MD5") ' Noncompliant
            Dim md5HashAlgorithm = HashAlgorithm.Create("MD5") ' Noncompliant

            Dim algoName = "MD5"
            Dim md5CryptoConfigVar = CryptoConfig.CreateFromName(algoName) ' Noncompliant
        End Sub

        Public Sub DSACalls(temp as Byte())
            Dim dsa = System.Security.Cryptography.DSA.Create() ' Noncompliant
            Dim dsaProvider = new DSACryptoServiceProvider() ' Noncompliant
            Dim dsaCryptoConfig = CryptoConfig.CreateFromName("DSA") ' Noncompliant
            Dim dsaAsymmetricAlgorithm = AsymmetricAlgorithm.Create("DSA") ' Noncompliant
        End Sub

        Public Sub HmaCalls(temp as Byte())
            Dim hmac = System.Security.Cryptography.HMAC.Create() ' Noncompliant
            Dim hmacSha1 = new HMACSHA1() ' Noncompliant
            Dim md5HashAlgorithm = HashAlgorithm.Create("MD5") ' Noncompliant
            Dim hmacmd5KeydHashAlgorithm = KeyedHashAlgorithm.Create("HMACMD5") ' Noncompliant
            Dim hmacmd5CryptoConfig = CryptoConfig.CreateFromName("HMACMD5") ' Noncompliant
            Dim hmacsha256 = System.Security.Cryptography.HMACSHA256.Create("HMACSHA256")
            Dim hmacsha256KeyedHashAlgorithm = System.Security.Cryptography.HMACSHA256.Create("HMACSHA256")
            Dim hmacsha256CryptoConfig = CryptoConfig.CreateFromName("HMACSHA256")
        End Sub

    End Class

End Namespace
