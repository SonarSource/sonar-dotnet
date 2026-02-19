Imports System
Imports System.Net
Imports System.Security
Imports System.Security.Cryptography

Namespace Tests.Diagnostics
    Class Program
        Public Sub Test()
            Dim passWord As String = "foo"
            Dim passKode As String = "a" 'Noncompliant {{"kode" detected here, make sure this is not a hard-coded credential.}}
            Dim passKodeKode As String = "a" 'Noncompliant
            Dim passKoDe As String = "a"    ' Error [BC30288] Local variable 'passKoDe' is already declared in the current block
            Dim x As String = "kode=a;kode=a" 'Noncompliant
            Dim x1 As String = "facal-faire=a;kode=a" 'Noncompliant
            Dim x2 As String = "x\*+?|}{][)(^$.# =something" ' Noncompliant {{"x\*+?|}{][)(^$.#" detected here, make sure this is not a hard-coded credential.}}
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

    Class NoWordBound
        ' This used to be an FN because the regex matched on word boundaries.
        Public Sub Test()
            Dim x As String = "*=something" ' Noncompliant
        End Sub
    End Class
End Namespace
