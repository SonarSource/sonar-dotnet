Imports System
Imports System.Collections.Generic
Imports System.Security.Cryptography
Imports Org.BouncyCastle.Crypto.Engines
Imports Org.BouncyCastle.Crypto.Modes

Namespace Tests.TestCases

    Public Class MyTripleDESCryptoServiceProvider
        Inherits TripleDES

        Public Overrides Sub GenerateKey()
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub GenerateIV()
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function CreateEncryptor(rgbKey() As Byte, rgbIV() As Byte) As ICryptoTransform
            Throw New NotImplementedException()
        End Function

        Public Overrides Function CreateDecryptor(rgbKey() As Byte, rgbIV() As Byte) As ICryptoTransform
            Throw New NotImplementedException()
        End Function

    End Class

    Public Class InsecureEncryptionAlgorithm

        Public Sub New()

            Using TripleDES As New MyTripleDESCryptoServiceProvider() ' Noncompliant    {{Use a strong cipher algorithm.}}
            '                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            '   ...
            End Using

            Using Des = New DESCryptoServiceProvider() ' Noncompliant
            '   ...
            End Using

            Using TripleDESalg = TripleDES.Create() ' Noncompliant
            '                    ^^^^^^^^^^^^^^^^^^
            '   ...
            End Using

            Using DesInstance = DES.Create("fgdsgsdfgsd") ' Noncompliant
            '   ...
            End Using

            Using Aes = new AesCryptoServiceProvider() ' Compliant
                '   ...
            End Using

            Using Rc21 = new RC2CryptoServiceProvider() ' Noncompliant
            '   ...
            End Using

            Using Rc22 = RC2.Create() ' Noncompliant
            '   ...
            End Using

            Dim Des1 As SymmetricAlgorithm = SymmetricAlgorithm.Create("DES") ' Noncompliant

            Des1 = SymmetricAlgorithm.Create("TripleDES") ' Noncompliant

            Des1 = SymmetricAlgorithm.Create("3DES") ' Noncompliant

            Dim Rc2Instance = SymmetricAlgorithm.Create("RC2") ' Noncompliant

            Dim Crypto = CryptoConfig.CreateFromName("DES") ' Noncompliant

            Dim AesFastEngineInstance = New AesFastEngine() ' Noncompliant
            '                               ^^^^^^^^^^^^^

            Dim BlockCipher1 = new GcmBlockCipher(new AesFastEngine()) ' Noncompliant

            Dim BlockCipher2 = new GcmBlockCipher(new AesEngine()) ' Compliant

            Dim Oid = CryptoConfig.MapNameToOID("DES") ' Compliant

        End Sub

        Public Sub NullConditionalIndexing(List As List(Of Integer))
            Dim Value As Integer = List?(0)
        End Sub

    End Class

End Namespace
