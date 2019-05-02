Imports System

Namespace Tests.Diagnostics
    Class Program
        Public Sub Test()
            Dim password As String = "foo" 'Noncompliant {{'password' detected in this expression, review this potentially hardcoded credential.}}
'               ^^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim foo As String, passwd As String = "a" 'Noncompliant {{'passwd' detected in this expression, review this potentially hardcoded credential.}}
'                              ^^^^^^^^^^^^^^^^^^^^^^
            Dim foo2 As String = "Password=123" 'Noncompliant
            Dim bar As String
            bar = "Password=p" 'Noncompliant
'           ^^^^^^^^^^^^^^^^^^
            foo = "password="
            foo = "passwordpassword"
            foo = "foo=1;password=1" 'Noncompliant
            foo = "foo=1password=1"

            Dim myPassword1 As String = Nothing
            Dim myPassword2 As String = ""
            Dim myPassword3 As String = "   "
            Dim myPassword4 As String = "foo" 'Noncompliant {{'password' detected in this expression, review this potentially hardcoded credential.}}

        End Sub
    End Class

    Class FalseNegatives
        Private password As String

        Public Sub Foo()
            Me.password = "foo" ' False Negative
            Configuration.Password = "foo" ' False Negative
            Me.password = Configuration.Password = "foo" ' False Negative
        End Sub

        Class Configuration
            Public Shared Property Password As String
        End Class
    End Class

End Namespace
