Imports System
Imports System.Security.Cryptography

Class InitializationVectorShouldBeRandom
    Public Sub SymetricAlgorithmCreateEncryptor()
        Dim initializationVectorConstant = New Byte(15) {}

        Using sa = SymmetricAlgorithm.Create("AES")
            Dim noParams As ICryptoTransform = sa.CreateEncryptor()                         ' Compliant - IV is automatically generated
            Dim defaultKeyAndIV = sa.CreateEncryptor(sa.Key, sa.IV)                         ' Compliant

            sa.GenerateKey()
            Dim generateIVNotCalled = sa.CreateEncryptor(sa.Key, sa.IV)
            Dim constantVector = sa.CreateEncryptor(sa.Key, initializationVectorConstant)   ' Noncompliant  {{Use a dynamically-generated, random IV.}}
            '                    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            constantVector = sa.CreateEncryptor(sa.Key, initializationVectorConstant)       ' Noncompliant

            sa.GenerateIV()
            Dim defaultConstructor = sa.CreateEncryptor()                                   ' Compliant
            Dim compliant = sa.CreateEncryptor(sa.Key, sa.IV)

            sa.KeySize = 12
            sa.CreateEncryptor()                                                            ' Compliant - updating key size does not change the status
            sa.CreateEncryptor(sa.Key, New CustomAlg().IV)
            sa.CreateEncryptor(sa.Key, New CustomAlg().Key)

            sa.IV = initializationVectorConstant
            sa.GenerateKey()

            Dim ivReplacedDefaultConstructor = sa.CreateEncryptor()                         ' Noncompliant
            Dim ivReplaced = sa.CreateEncryptor(sa.Key, sa.IV)                              ' Noncompliant
            sa.IV = New Byte(15) {}
            sa.CreateEncryptor(sa.Key, sa.IV)                                               ' Noncompliant
            ClassWithStaticProperty.Count = 10
        End Using

    End Sub

    Public Sub CallEncryptorAgainWithoutInput()
        Dim initializationVectorConstant = New Byte(15) {}

        Using sa As SymmetricAlgorithm = SymmetricAlgorithm.Create("AES")
            sa.CreateEncryptor(sa.Key, initializationVectorConstant)                        ' Noncompliant
            sa.CreateEncryptor()
        End Using

    End Sub
    
    Public Sub TestWithDifferentCase()
 
        Dim initializationVectorConstant = New Byte(15) {}

        Using sa As SymmetricAlgorithm = SymmetricAlgorithm.Create("AES")
            sa.iv = initializationVectorConstant
            sa.CREATEENCRYPTOR()                        ' Noncompliant
            sa.CreateEncryptor()                        ' Noncompliant
        End Using
    
    End Sub

    Public Sub CallEncryptorWithIVProperty(ByVal condition As Boolean)

        Dim initializationVectorConstant = New Byte(15) {}

        Dim sa As SymmetricAlgorithm = SymmetricAlgorithm.Create("AES")
        Dim sa2 As SymmetricAlgorithm = SymmetricAlgorithm.Create("AES")

        sa2.IV = New Byte(15) {}

        sa.CreateEncryptor(sa.Key, sa2.IV)                                                  ' Noncompliant

        Dim x = sa2.IV
        Dim y = x

        sa.CreateEncryptor(sa.Key, x)                                                       ' Noncompliant
        sa.CreateEncryptor(sa.Key, y)                                                       ' Noncompliant
        sa.CreateEncryptor(sa.Key, (If(condition, sa2, sa2)).IV)                            ' FN
    End Sub

    Public Sub RandomIsNotCompliant()
        Dim initializationVectorWeakBytes = New Byte(15) {}
        Call New Random().NextBytes(initializationVectorWeakBytes)

        Dim sa = SymmetricAlgorithm.Create("AES")
        Dim encryptor = sa.CreateEncryptor(sa.Key, initializationVectorWeakBytes)           ' Noncompliant
    End Sub

    Public Sub CustomGenerationNotCompliant()
        Dim initializationVectorWeakFor = New Byte(15) {}

        Dim rnd = New Random()
        For i = 0 To initializationVectorWeakFor.Length - 1
            initializationVectorWeakFor(i) = CByte(rnd.Next() Mod 256)
        Next

        Dim sa = SymmetricAlgorithm.Create("AES")
        sa.CreateEncryptor(sa.Key, initializationVectorWeakFor)                             ' Noncompliant
    End Sub

    Public Sub UsingRNGCryptoServiceProviderIsCompliant(ByVal condition As Boolean)

        Dim initializationVectorConstant = New Byte(15) {}
        Dim initializationVectorRng = New Byte(15) {}
        Dim initializationVectorRngNonZero = New Byte(15) {}
        Dim initializationVectorInExpression = New Byte(15) {}

        Using rng = New RNGCryptoServiceProvider()
            rng.GetBytes(initializationVectorRng)
            rng.GetNonZeroBytes(initializationVectorRngNonZero)
            rng.GetBytes((If(condition, initializationVectorInExpression, initializationVectorInExpression)))
        End Using

        Using sa = SymmetricAlgorithm.Create("AES")

            sa.GenerateKey()
            Dim fromRng = sa.CreateEncryptor(sa.Key, initializationVectorRng)
            Dim fromRngNonZero = sa.CreateEncryptor(sa.Key, initializationVectorRngNonZero)

            sa.GenerateIV()
            Dim fromGenerateIV = sa.CreateEncryptor(sa.Key, sa.IV)
            Dim fromRandomGeneratorWithExpression = sa.CreateEncryptor(sa.Key, initializationVectorInExpression) ' Noncompliant
            sa.CreateDecryptor(sa.Key, initializationVectorConstant)                        ' Compliant, not relevant for decrypting
        End Using

    End Sub

    ' https://github.com/SonarSource/sonar-dotnet/issues/4274
    Public Sub ImplicitlyTypedArrayWithoutNew()
        Dim initializationVectorConstants As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
        Dim initializationVector As Byte() = {Rnd(), Rnd(), Rnd()}
        Using aes As AesCryptoServiceProvider = New AesCryptoServiceProvider()
            Dim encryptor = aes.CreateEncryptor(aes.Key, initializationVectorConstants)     ' Noncompliant
            encryptor = aes.CreateEncryptor(aes.Key, initializationVector)                  ' Noncompliant
        End Using
    End Sub

    Public Sub ImplicitlyTypedArrayWithNewWithConstantsInside()
        Dim initializationVectorConstants = New Byte() {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
        Dim initializationVector = New Byte() {Rnd()}
        Using aes As AesCryptoServiceProvider = New AesCryptoServiceProvider()
            Dim encryptor = aes.CreateEncryptor(aes.Key, initializationVectorConstants)     ' Noncompliant
            encryptor = aes.CreateEncryptor(aes.Key, initializationVector)                  ' Noncompliant
        End Using
    End Sub

    Public Sub ImplicitlyTypedArrayNonByte()
        Dim intArray = {1, 2, 3}
        Dim byteArray = New Byte(intArray.Length * 4 - 1) {}
        Buffer.BlockCopy(intArray, 0, byteArray, 0, byteArray.Length)
        Using aes As AesCryptoServiceProvider = New AesCryptoServiceProvider()
            Dim encryptor = aes.CreateEncryptor(aes.Key, byteArray)                         ' Noncompliant
        End Using
    End Sub

    Public Sub CollectionInitializer()
        Dim listWithConstant As List(Of Byte) = New List(Of Byte) From {&H0}
        Dim list As List(Of Byte) = New List(Of Byte) From {Rnd()}

        Using aes As AesCryptoServiceProvider = New AesCryptoServiceProvider()
            Dim encryptor As ICryptoTransform = aes.CreateEncryptor(aes.Key, listWithConstant.ToArray())    ' FN
            encryptor = aes.CreateEncryptor(aes.Key, list.ToArray())                                        ' FN
        End Using
    End Sub

    Public Sub InsideObjectInitializer()
        Dim anonymous = New With {
            .IV = New Byte() {&H0},
            .Key = New Byte() {&H0}
        }
        Using aes As AesCryptoServiceProvider = New AesCryptoServiceProvider()
            Dim encryptor = aes.CreateEncryptor(aes.Key, anonymous.IV)                 ' FN https://github.com/SonarSource/sonar-dotnet/issues/4555
        End Using
    End Sub

    Public Sub DifferentCases()
        Dim alg = New CustomAlg()
        alg.IV = New Byte(15) {}
    End Sub

    Private Function Rnd() As Byte
        Dim rand = New Random()
        Dim bytes = New Byte(0) {}
        rand.NextBytes(bytes)
        Return bytes(0)
    End Function
End Class

Public Class CodeWhichDoesNotCompile
    Public Sub Check()
        Dim initializationVectorConstant = New Byte(15) {}

        Using sa As FakeUnresolvedType = SymmetricAlgorithm.Create("AES")                                   ' Error [BC30002]
            sa.IV = initializationVectorConstant
        End Using
    End Sub

End Class

Public Class CustomAlg

    Public Overridable Property IV As Byte()
    Public Overridable Property Key As Byte()

End Class

Public Class CustomAes
    Inherits Aes

    Public Overrides Function CreateDecryptor(rgbKey As Byte(), rgbIV As Byte()) As ICryptoTransform
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateEncryptor(rgbKey As Byte(), rgbIV As Byte()) As ICryptoTransform
        Throw New NotImplementedException()
    End Function

    Public Overrides Sub GenerateIV()
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub GenerateKey()
        Throw New NotImplementedException()
    End Sub

End Class

Public Class SymmetricalEncryptorWrapper
    Private ReadOnly algorithm As SymmetricAlgorithm

    Public Sub New()
        algorithm = Aes.Create()
    End Sub

    Public Sub GenerateIV()
        algorithm.GenerateIV()
    End Sub

    Public Function CreateEncryptor() As ICryptoTransform
        Return algorithm.CreateEncryptor()
    End Function
End Class

Public Class ClassWithStaticProperty

    Public Shared Property Count As Integer

End Class
