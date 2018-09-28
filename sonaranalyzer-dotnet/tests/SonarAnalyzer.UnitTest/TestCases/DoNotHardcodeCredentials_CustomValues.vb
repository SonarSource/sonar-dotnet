Imports System

Namespace Tests.Diagnostics
    Class Program
        Public Sub Test()
            Dim passWord As String = "foo"
            Dim passKode As String = "a" 'Noncompliant {{Remove hard-coded password(s): 'kode'.}}
            Dim passKodeKode As String = "a" 'Noncompliant {{Remove hard-coded password(s): 'kode'.}}
            Dim passKoDe As String = "a"
            Dim x As String = "kode=a;kode=a" 'Noncompliant {{Remove hard-coded password(s): 'kode'.}}
            Dim x2 As String = "facal-faire=a;kode=a" 'Noncompliant {{Remove hard-coded password(s): 'facal-faire, kode'.}}
        End Sub
    End Class
End Namespace
