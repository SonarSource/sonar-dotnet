Imports System

Namespace Tests.Diagnostics
    Class Program
        Public Sub Test()
            Dim passWord As String = "foo"
            Dim passKode As String = "a" 'Noncompliant {{'kode' detected in this expression, review this potentially hardcoded credential.}}
            Dim passKodeKode As String = "a" 'Noncompliant {{'kode' detected in this expression, review this potentially hardcoded credential.}}
            Dim passKoDe As String = "a"    ' Error [BC30288] Local variable 'passKoDe' is already declared in the current block
            Dim x As String = "kode=a;kode=a" 'Noncompliant {{'kode' detected in this expression, review this potentially hardcoded credential.}}
            Dim x2 As String = "facal-faire=a;kode=a" 'Noncompliant {{'facal-faire, kode' detected in this expression, review this potentially hardcoded credential.}}
        End Sub
    End Class
End Namespace
