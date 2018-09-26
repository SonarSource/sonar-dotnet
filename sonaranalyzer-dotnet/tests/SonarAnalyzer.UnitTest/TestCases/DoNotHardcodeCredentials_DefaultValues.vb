Imports System

Namespace Tests.Diagnostics
    Class Program
        Public Sub Test()
            Dim password As String = "foo" 'Noncompliant {{Remove hard-coded password(s): 'password'.}}
'               ^^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim foo As String, passwd As String = "a" 'Noncompliant {{Remove hard-coded password(s): 'passwd'.}}
'                              ^^^^^^^^^^^^^^^^^^^^^^
            Dim foo2 As String = "Password=123" 'Noncompliant
            Dim bar As String
            bar = "Password=p" 'Noncompliant
'           ^^^^^^^^^^^^^^^^^^
            foo = "password="
            foo = "passwordpassword"
            foo = "foo=1;password=1" 'Noncompliant
            foo = "foo=1password=1"
        End Sub
    End Class
End Namespace
