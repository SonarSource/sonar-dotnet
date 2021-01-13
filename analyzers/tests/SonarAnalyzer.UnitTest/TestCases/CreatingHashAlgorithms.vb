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

            Dim HMACCreate = HMAC.Create() ' Noncompliant
            Dim HMACCreateWithParam = HMAC.Create("HMACMD5") ' Noncompliant

            Dim HMACMD5 = new HMACMD5() ' Noncompliant
            Dim HMACMD5Create = HMACMD5.Create() ' Noncompliant
            Dim HMACMD5CreateWithParam = HMACMD5.Create("HMACMD5") ' Noncompliant
            Dim HMACMD5KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACMD5") ' Noncompliant
            Dim HMACMD5CryptoConfig = CryptoConfig.CreateFromName("HMACMD5") ' Noncompliant

            Dim HMACRIPEMD160 = new HMACRIPEMD160() ' Noncompliant
            Dim HMACRIPEMD160Create = HMACMD5.Create() ' Noncompliant
            Dim HMACRIPEMD160CreateWithParam = HMACMD5.Create("HMACRIPEMD160") ' Noncompliant
            Dim HMACRIPEMD160KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACRIPEMD160") ' Noncompliant
            Dim HMACRIPEMD160CryptoConfig = CryptoConfig.CreateFromName("HMACRIPEMD160") ' Noncompliant

            Dim HMACSHA1 = new HMACSHA1() ' Noncompliant

            Dim HMACSHA256Create = HMACSHA256.Create("HMACSHA256")
            Dim HMACSHA256KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACSHA256")
            Dim HMACSHA256CryptoConfig = CryptoConfig.CreateFromName("HMACSHA256")

            Dim MD5Cng = new MD5Cng() ' Noncompliant
            Dim MD5CryptoServiceProvider = new MD5CryptoServiceProvider() ' Noncompliant
            Dim MD5CryptoConfig = CryptoConfig.CreateFromName("MD5") ' Noncompliant
            Dim MD5HashAlgorithm = HashAlgorithm.Create("MD5") ' Noncompliant
            Dim MD5Create = MD5.Create() ' Noncompliant
            Dim MD5CreateWithParam = MD5.Create("MD5") ' Noncompliant

            Dim RIPEMD160Managed  = new RIPEMD160Managed() ' Noncompliant
            Dim RIPEMD160ManagedCreate = RIPEMD160Managed.Create("RIPEMD160Managed") ' Noncompliant
            Dim RIPEMD160ManagedHashAlgorithm = HashAlgorithm.Create("RIPEMD160Managed") ' Noncompliant
            Dim RIPEMD160ManagedCryptoConfig = CryptoConfig.CreateFromName("RIPEMD160Managed") ' Noncompliant

            Dim RIPEMD160Create = RIPEMD160.Create() ' Noncompliant
            Dim RIPEMD160CreateWithParam = RIPEMD160.Create("RIPEMD160") ' Noncompliant
            Dim RIPEMD160HashAlgorithm = HashAlgorithm.Create("RIPEMD160") ' Noncompliant
            Dim RIPEMD160CryptoConfig = CryptoConfig.CreateFromName("RIPEMD160") ' Noncompliant

            Dim SHA1Create = SHA1.Create() ' Noncompliant
            Dim SHA1CreateWithParam = SHA1.Create("SHA1") ' Noncompliant
            Dim SHA1Managed = new SHA1Managed() ' Noncompliant
            Dim SHA1HashAlgorithm = HashAlgorithm.Create("SHA1") ' Noncompliant
            Dim SHA1CryptoServiceProvider = new SHA1CryptoServiceProvider() ' Noncompliant

            Dim SHA256Managed = new SHA256Managed()
            Dim SHA256ManagedHashAlgorithm = HashAlgorithm.Create("SHA256Managed")
            Dim SHA256ManagedCryptoConfig = CryptoConfig.CreateFromName("SHA256Managed")

            Dim hashAlgo = HashAlgorithm.Create()

            Dim algoName = "MD5"
            Dim MD5Var = CryptoConfig.CreateFromName(algoName) ' Noncompliant

            algoName = "SHA256Managed"
            Dim SHA256ManagedVar = CryptoConfig.CreateFromName(algoName)
        End Sub

    End Class

End Namespace
