Imports System
Imports System.Net
Imports System.Security
Imports System.Security.Cryptography

Namespace Tests.Diagnostics
    Class Program
        Public Sub Test()
            Dim passWord As String = "foo"
            Dim passKode As String = "a" 'Noncompliant {{Make sure hard-coded credential is safe.}}
            Dim passKodeKode As String = "a" 'Noncompliant
            Dim passKoDe As String = "a"    ' Error [BC30288] Local variable 'passKoDe' is already declared in the current block
            Dim x As String = "kode=a;kode=a" 'Noncompliant
            Dim x1 As String = "facal-faire=a;kode=a" 'Noncompliant
            Dim x2 as String = "x\*+?|}{][)(^$.# =something" ' Noncompliant
        End Sub

        Public Sub StandardAPI(secureString As SecureString, nonHardcodedPassword As String, byteArray As Byte(), cspParams As CspParameters)
            Dim networkCredential = New NetworkCredential()
            networkCredential.Password = nonHardcodedPassword
            networkCredential.Domain = "hardcodedDomain"
            networkCredential = New NetworkCredential("username", secureString)
            networkCredential = New NetworkCredential("username", nonHardcodedPassword, "domain")
            Dim passwordDeriveBytes = New PasswordDeriveBytes(nonHardcodedPassword, byteArray)
            passwordDeriveBytes = New PasswordDeriveBytes(New Byte() {1}, byteArray, "strHashName", 1)

            networkCredential = New NetworkCredential("username", "hardcoded", "domain") 'Noncompliant
            networkCredential.Password = "hardcoded" 'Noncompliant
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1, cspParams) 'Noncompliant
        End Sub
    End Class

    Class FalseNegatives
        Public Sub Test()
            Dim x as String = "*=something" ' FN - current regex expects \b (word boundary) at the beginning
        End Sub
    End Class
End Namespace
