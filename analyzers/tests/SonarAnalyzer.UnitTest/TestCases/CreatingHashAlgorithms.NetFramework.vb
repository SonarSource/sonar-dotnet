Imports System.Security.Cryptography

Namespace Tests.Diagnostics

    Public Class InsecureHashAlgorithm

        Public Sub Hash(temp as Byte())
            Dim HMACRIPEMD160 = new HMACRIPEMD160() ' Noncompliant
            Dim HMACRIPEMD160Create = HMACMD5.Create() ' Noncompliant
            Dim HMACRIPEMD160CreateWithParam = HMACMD5.Create("HMACRIPEMD160") ' Noncompliant
            Dim HMACRIPEMD160KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACRIPEMD160") ' Noncompliant
            Dim HMACRIPEMD160KeyedHashAlgorithmWithNamespace = KeyedHashAlgorithm.Create("System.Security.Cryptography.HMACRIPEMD160") ' Noncompliant
            Dim HMACRIPEMD160CryptoConfig = CryptoConfig.CreateFromName("HMACRIPEMD160") ' Noncompliant

            Dim MD5Cng = new MD5Cng() ' Noncompliant

            Dim RIPEMD160Managed  = new RIPEMD160Managed() ' Noncompliant

            Dim RIPEMD160Create = RIPEMD160.Create() ' Noncompliant
            Dim RIPEMD160CreateWithParam = RIPEMD160.Create("RIPEMD160") ' Noncompliant
            Dim RIPEMD160HashAlgorithm = HashAlgorithm.Create("RIPEMD160") ' Noncompliant
            Dim RIPEMD160HashAlgorithmWithNamespace = HashAlgorithm.Create("System.Security.Cryptography.RIPEMD160") ' Noncompliant
            Dim RIPEMD160CryptoConfig = CryptoConfig.CreateFromName("RIPEMD160") ' Noncompliant
        End Sub

    End Class

End Namespace
