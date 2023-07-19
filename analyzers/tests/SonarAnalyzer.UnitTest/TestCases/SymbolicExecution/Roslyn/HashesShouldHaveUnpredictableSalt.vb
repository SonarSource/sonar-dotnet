Imports System
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports AliasedPasswordDeriveBytes = System.Security.Cryptography.PasswordDeriveBytes

Class Program
    Private Const passwordString As String = "Secret"
    Private cspParams As CspParameters = New CspParameters()
    Private ReadOnly passwordBytes As Byte() = Encoding.UTF8.GetBytes(passwordString)

    ' Out of the two issues (salt is too short vs. salt is predictable) being predictable is the more serious one.
    ' If both issues are present then the rule's message will reflect on the salt being predictable.
    Public Sub ShortAndConstantSaltIsNotCompliant()
        Dim shortAndConstantSalt = New Byte(14) {}
        Dim pdb1 = New PasswordDeriveBytes(passwordBytes, shortAndConstantSalt)                                                      ' Noncompliant {{Make this salt unpredictable.}}
        '                                                 ^^^^^^^^^^^^^^^^^^^^
        Dim pdb2 = New PasswordDeriveBytes(salt:=shortAndConstantSalt, password:=passwordBytes)                                      ' Noncompliant
        '                                        ^^^^^^^^^^^^^^^^^^^^
        Dim pdb3 = New PasswordDeriveBytes(passwordString, shortAndConstantSalt)                                                     ' Noncompliant
        Dim pdb4 = New PasswordDeriveBytes(passwordBytes, shortAndConstantSalt, cspParams)                                           ' Noncompliant
        Dim pdb5 = New PasswordDeriveBytes(passwordString, shortAndConstantSalt, cspParams)                                          ' Noncompliant
        Dim pdb6 = New PasswordDeriveBytes(passwordBytes, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000)                 ' Noncompliant
        Dim pdb7 = New PasswordDeriveBytes(passwordString, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000)                ' Noncompliant
        Dim pdb8 = New PasswordDeriveBytes(passwordBytes, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams)      ' Noncompliant
        Dim pdb9 = New PasswordDeriveBytes(passwordString, shortAndConstantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams)     ' Noncompliant

        Dim pbkdf2a = New Rfc2898DeriveBytes(passwordString, shortAndConstantSalt)                                                   ' Noncompliant {{Make this salt unpredictable.}}
        Dim pbkdf2b = New Rfc2898DeriveBytes(passwordString, shortAndConstantSalt, 1000)                                             ' Noncompliant
        Dim pbkdf2c = New Rfc2898DeriveBytes(passwordBytes, shortAndConstantSalt, 1000)                                              ' Noncompliant
        Dim pbkdf2d = New Rfc2898DeriveBytes(passwordString, shortAndConstantSalt, 1000, HashAlgorithmName.SHA512)                   ' Noncompliant
    End Sub

    Public Sub ConstantHashIsNotCompliant()
        Dim constantSalt = New Byte(15) {}
        Dim pdb1 = New PasswordDeriveBytes(passwordBytes, constantSalt)                                                              ' Noncompliant {{Make this salt unpredictable.}}
        Dim pdb2 = New PasswordDeriveBytes(passwordString, constantSalt)                                                             ' Noncompliant
        Dim pdb3 = New PasswordDeriveBytes(passwordBytes, constantSalt, cspParams)                                                   ' Noncompliant
        Dim pdb4 = New PasswordDeriveBytes(passwordString, constantSalt, cspParams)                                                  ' Noncompliant
        Dim pdb5 = New PasswordDeriveBytes(passwordBytes, constantSalt, HashAlgorithmName.SHA512.Name, 1000)                         ' Noncompliant
        Dim pdb6 = New PasswordDeriveBytes(passwordString, constantSalt, HashAlgorithmName.SHA512.Name, 1000)                        ' Noncompliant
        Dim pdb7 = New PasswordDeriveBytes(passwordBytes, constantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams)              ' Noncompliant
        Dim pdb8 = New PasswordDeriveBytes(passwordString, constantSalt, HashAlgorithmName.SHA512.Name, 1000, cspParams)             ' Noncompliant

        Dim pbkdf2a = New Rfc2898DeriveBytes(passwordString, constantSalt)                                                           ' Noncompliant {{Make this salt unpredictable.}}
        Dim pbkdf2b = New Rfc2898DeriveBytes(passwordString, constantSalt, 1000)                                                     ' Noncompliant
        Dim pbkdf2c = New Rfc2898DeriveBytes(passwordBytes, constantSalt, 1000)                                                      ' Noncompliant
        Dim pbkdf2d = New Rfc2898DeriveBytes(passwordString, constantSalt, 1000, HashAlgorithmName.SHA512)                           ' Noncompliant
    End Sub

    Public Sub RNGCryptoServiceProviderIsCompliant()
        Dim getBytesSalt = New Byte(15) {}

        Using rng = New RNGCryptoServiceProvider()
            rng.GetBytes(getBytesSalt)
            Dim pdb1 = New PasswordDeriveBytes(passwordBytes, getBytesSalt)
            Dim pbkdf1 = New Rfc2898DeriveBytes(passwordString, getBytesSalt)

            Dim getNonZeroBytesSalt = New Byte(15) {}
            rng.GetNonZeroBytes(getNonZeroBytesSalt)
            Dim pdb2 = New PasswordDeriveBytes(passwordBytes, getBytesSalt)
            Dim pbkdf2 = New Rfc2898DeriveBytes(passwordString, getBytesSalt)

            Dim shortSalt = New Byte(14) {}
            rng.GetBytes(shortSalt)
            Dim pdb3 = New PasswordDeriveBytes(passwordBytes, shortSalt)     ' Noncompliant {{Make this salt at least 16 bytes.}}
            Dim pbkdf3 = New Rfc2898DeriveBytes(passwordString, shortSalt)   ' Noncompliant
        End Using
    End Sub

    Public Sub RandomNumberGeneratorIsCompliant()
        Dim getBytesSalt = New Byte(15) {}

        Using rng = RandomNumberGenerator.Create()
            rng.GetBytes(getBytesSalt)
            Dim pdb1 = New PasswordDeriveBytes(passwordBytes, getBytesSalt)
            Dim pbkdf1 = New Rfc2898DeriveBytes(passwordString, getBytesSalt)

            Dim getNonZeroBytesSalt = New Byte(15) {}
            rng.GetNonZeroBytes(getNonZeroBytesSalt)
            Dim pdb2 = New PasswordDeriveBytes(passwordBytes, getBytesSalt)
            Dim pbkdf2 = New Rfc2898DeriveBytes(passwordString, getBytesSalt)

            Dim shortSalt = New Byte(14) {}
            rng.GetBytes(shortSalt)
            Dim pdb3 = New PasswordDeriveBytes(passwordBytes, shortSalt)     ' Noncompliant {{Make this salt at least 16 bytes.}}
            Dim pbkdf3 = New Rfc2898DeriveBytes(passwordString, shortSalt)   ' Noncompliant
        End Using
    End Sub

    ' System.Random generates pseudo-random numbers, therefore it's not suitable to generate crypthoraphically secure random numbers.
    Public Sub SystemRandomIsNotCompliant()
        Dim rnd = New Random()
        Dim saltCustom = New Byte(15) {}
        For i = 0 To saltCustom.Length - 1
            saltCustom(i) = CByte(rnd.Next(255))
        Next
        Dim pdb = New PasswordDeriveBytes(passwordBytes, saltCustom)         ' Noncompliant
    End Sub

    Public Sub ToArrayInvocation()
        Dim noncompliantSalt = New Byte(14) {}
        Dim pdb1 = New PasswordDeriveBytes(passwordBytes, noncompliantSalt.ToArray())             ' FN
        Dim pdb2 = New PasswordDeriveBytes(passwordBytes, noncompliantSalt.ToList().ToArray())    ' FN

        Dim compliantSalt = New Byte(15) {}
        Dim rng = RandomNumberGenerator.Create()
        rng.GetBytes(compliantSalt)
        Dim pdb3 = New PasswordDeriveBytes(passwordBytes, compliantSalt.ToArray())                ' Compliant
    End Sub

    Public Sub SaltAsParameter(salt As Byte())
        Dim pdb = New PasswordDeriveBytes(passwordBytes, salt)               ' Compliant, we know nothing about salt
        Dim pbkdf = New Rfc2898DeriveBytes(passwordString, salt)             ' Compliant, we know nothing about salt
    End Sub

    Public Sub EncodingGetBytesWithStringLiterals(saltAsText As String)
        Dim constantSalt1 = Encoding.UTF8.GetBytes("HardcodedText")
        Dim constantSalt2 = Encoding.Unicode.GetBytes(CStr("HardcodedText"))
        Dim pdb1 = New PasswordDeriveBytes(passwordBytes, constantSalt1)     ' Noncompliant
        Dim pdb2 = New PasswordDeriveBytes(passwordBytes, constantSalt2)     ' Noncompliant

        Dim constantSalt3 = New Byte(15) {}
        Encoding.UTF8.GetBytes("HardcodedText", 0, 1, constantSalt3, 0)
        Dim pdb3 = New PasswordDeriveBytes(passwordBytes, constantSalt3)     ' Noncompliant

        Dim hardcodedTextInLocalVariable = "HardcodedText"
        Dim constantSalt4 = Encoding.UTF8.GetBytes(hardcodedTextInLocalVariable)
        Dim pdb4 = New PasswordDeriveBytes(passwordBytes, constantSalt4)     ' FN

        Const constantText = "HardcodedText"
        Dim constantSalt5 = Encoding.UTF8.GetBytes(constantText)
        Dim pdb5 = New PasswordDeriveBytes(passwordBytes, constantSalt5)     ' FN

        Dim notConstantSalt = Encoding.UTF8.GetBytes(saltAsText)
        Dim pdb6 = New PasswordDeriveBytes(passwordBytes, notConstantSalt)   ' Compliant - we don't know where the argument is coming from
    End Sub


    Public Sub ImplicitSaltIsCompliant(password As String)
        Dim withAutomaticSalt1 = New Rfc2898DeriveBytes(passwordString, saltSize:=16)
        Dim withAutomaticSalt2 = New Rfc2898DeriveBytes(passwordString, 16, 1000)
        Dim withAutomaticSalt3 = New Rfc2898DeriveBytes(passwordString, 16, 1000, HashAlgorithmName.SHA512)

        Dim withAutomaticSalt4 = New Rfc2898DeriveBytes(passwordString, saltSize:=16)
        Dim withAutomaticSalt5 = New Rfc2898DeriveBytes(passwordString, 16, 1000)
        Dim withAutomaticSalt6 = New Rfc2898DeriveBytes(passwordString, 16, 1000, HashAlgorithmName.SHA512)
    End Sub

    Public Sub Conditional(arg As Integer)
        Dim rng = RandomNumberGenerator.Create()
        Dim salt = New Byte(15) {}
        If arg = 1 Then
            rng.GetBytes(salt)
            Dim pdb1 = New PasswordDeriveBytes(passwordBytes, salt)                                     ' Compliant
        End If
        Dim pdb2 = New PasswordDeriveBytes(passwordBytes, salt)                                         ' Noncompliant {{Make this salt unpredictable.}}

        Dim noncompliantSalt = New Byte(15) {}
        Dim compliantSalt = New Byte(15) {}
        rng.GetBytes(compliantSalt)
        Dim salt3 = If(arg = 2, compliantSalt, noncompliantSalt)
        Dim pdb3 = New PasswordDeriveBytes(passwordBytes, salt3)                                        ' Noncompliant

        Dim salt4 = If(True, compliantSalt, noncompliantSalt)
        Dim pdb4 = New PasswordDeriveBytes(passwordBytes, salt4)                                        ' Compliant

        Dim pdb5 = New PasswordDeriveBytes(passwordBytes, If(True, New Byte(15) {}, compliantSalt))     ' Noncompliant
        Dim pdb6 = New PasswordDeriveBytes(passwordBytes, If(True, compliantSalt, New Byte(15) {}))     ' Compliant
    End Sub

    Public Sub TryCatchFinally()
        Dim salt = New Byte(15) {}
        Try
            Dim pdb1 = New PasswordDeriveBytes(passwordBytes, salt)                                     ' Noncompliant
        Catch
            Dim pdb2 = New PasswordDeriveBytes(passwordBytes, salt)                                     ' Noncompliant
        Finally
            Dim rng = RandomNumberGenerator.Create()
            rng.GetBytes(salt)
        End Try
        Dim pdb3 = New PasswordDeriveBytes(passwordBytes, salt)                                         ' Compliant
    End Sub

    Public Sub AssignedToAnotherVariable()
        Dim pdb = New PasswordDeriveBytes(passwordBytes, New Byte(15) {})                               ' Noncompliant
    End Sub

    Public Sub Lambda()
        Dim a As Action(Of Byte()) = Sub(passwordBytes)
                                         Dim shortSalt = New Byte(14) {}
                                         Dim pdb = New PasswordDeriveBytes(passwordBytes, shortSalt)    ' Noncompliant
                                     End Sub
    End Sub

    Public Sub AliasedTypeAndFullName()
        Dim shortAndConstantSalt = New Byte(14) {}
        Dim pdb1 = New AliasedPasswordDeriveBytes(passwordBytes, shortAndConstantSalt)                              ' Noncompliant
        Dim pdb2 = New System.Security.Cryptography.PasswordDeriveBytes(passwordBytes, shortAndConstantSalt)        ' Noncompliant
        Dim pdb3 = New PasswordDeriveBytes(passwordBytes, shortAndConstantSalt)                                     ' Noncompliant
        Dim pdb4 = New PasswordDeriveBytes(passwordBytes, shortAndConstantSalt)                                     ' Noncompliant
    End Sub

    Public Sub ByteArrayCases(passwordBytes As Byte())
        Dim rng = RandomNumberGenerator.Create()

        Dim multiDimensional = New Byte(0)() {}
        rng.GetBytes(multiDimensional(0))
        Dim pdb1 = New PasswordDeriveBytes(passwordBytes, multiDimensional(0))                                ' FN, not supported

        Dim shortArray = New Byte() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14}
        rng.GetBytes(shortArray)
        Dim pdb2 = New PasswordDeriveBytes(passwordBytes, shortArray)                                         ' Noncompliant

        Dim longEnoughArray = New Byte() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}
        rng.GetBytes(longEnoughArray)
        Dim pdb3 = New PasswordDeriveBytes(passwordBytes, longEnoughArray)                                    ' Compliant

        Dim evenLongerArray = New Byte() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16}
        rng.GetBytes(evenLongerArray)
        Dim pdb4 = New PasswordDeriveBytes(passwordBytes, evenLongerArray)                                    ' Compliant

        Dim pdb5 = New PasswordDeriveBytes(passwordBytes, GetSalt())                                          ' Compliant

        Dim returnedByMethod = GetSalt()
        Dim pdb6 = New PasswordDeriveBytes(passwordBytes, returnedByMethod)                                   ' Compliant
    End Sub

    Public Sub UsingCustomPasswordDeriveClass()
        Dim salt = New Byte(15) {}
        Dim pdb1 = New CustomPasswordDeriveClass("somepassword", salt)                                        ' Noncompliant

        Dim rng = RandomNumberGenerator.Create()
        rng.GetBytes(salt)
        Dim pdb2 = New CustomPasswordDeriveClass("somepassword", salt)                                        ' Compliant
    End Sub

    Private Function GetSalt() As Byte()
        Return Nothing
    End Function

    Private Class CustomPasswordDeriveClass
        Inherits DeriveBytes
        Public Sub New(password As String, salt As Byte())
        End Sub

        Public Overrides Function GetBytes(cb As Integer) As Byte()
            Return Nothing
        End Function

        Public Overrides Sub Reset()
        End Sub
    End Class
End Class

Public Class FieldsAndConstants
    Private Const UnsafeSaltSize As Integer = 15
    Private Const SafeSaltSize As Integer = 16
    Private saltField As Byte() = New Byte(15) {} ' Salt as field is not tracked by the SE engine

    Public Sub SaltStoredInField(passwordBytes As Byte())
        Dim rng = RandomNumberGenerator.Create()
        Dim pdb1 = New PasswordDeriveBytes(passwordBytes, saltField)              ' FN
        Dim pdb2 = New Rfc2898DeriveBytes(passwordBytes, saltField, SafeSaltSize) ' Compliant

        saltField = New Byte(16) {}
        rng.GetBytes(saltField)
        Dim pdb3 = New PasswordDeriveBytes(passwordBytes, saltField)              ' Compliant

        saltField = New Byte(15) {}
        Dim pdb4 = New PasswordDeriveBytes(passwordBytes, saltField)              ' Noncompliant
    End Sub

    Public Sub SaltSizeFromConstantField(passwordBytes As Byte())
        Dim rng = RandomNumberGenerator.Create()

        Dim unsafeSalt = New Byte(UnsafeSaltSize) {}
        rng.GetBytes(unsafeSalt)
        Dim pdb1 = New PasswordDeriveBytes(passwordBytes, unsafeSalt)             ' FN

        Dim safeSalt = New Byte(SafeSaltSize) {}
        rng.GetBytes(safeSalt)
        Dim pdb2 = New PasswordDeriveBytes(passwordBytes, safeSalt)               ' Compliant
    End Sub
End Class
