Imports System.Security.Cryptography

Namespace Tests.Diagnostics

    Class RSASignaturePaddingTest

        Private Sub SignAndVerify(ByVal data As Byte(), ByVal signature As Byte(), ByVal genericRSA As RSA, ByVal padding As RSASignaturePadding)
            Dim rsaCsp As RSACryptoServiceProvider = New RSACryptoServiceProvider()
            Dim rsaCng As RSACng = New RSACng()
            Dim rsaCreate As RSA = RSA.Create()

            ' RSACryptoServiceProvider can never use RSASSA-PSS, on any TFM, so this always raises.
            rsaCsp.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ' Noncompliant {{Use secure mode and padding scheme. RSACryptoServiceProvider does not support RSASSA-PSS, switch to RSACng.}}
            rsaCsp.SignHash(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ' Noncompliant

            ' RSACng has supported RSASSA-PSS since .NET Framework 4.6, so this is a reachable fix and always raises.
            rsaCng.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ' Noncompliant {{Use secure mode and padding scheme.}}
            rsaCng.SignHash(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ' Noncompliant

            ' RSA.Create() resolves to RSACryptoServiceProvider on .NET Framework, but since the receiver's static
            ' type here is only the abstract RSA, we can't be certain - so we stay conservative and don't raise.
            rsaCreate.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ' Compliant, unresolved receiver type on .NET Framework
            genericRSA.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ' Compliant, unresolved receiver type on .NET Framework

            ' Verifiers can't choose the padding scheme - it has to match whatever the signer used - so this is not actionable.
            rsaCsp.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ' Compliant
        End Sub

    End Class

End Namespace
