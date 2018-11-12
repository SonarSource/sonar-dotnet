Imports System.Web.Mvc

Public Class Foo
    Inherits Controller

    Public Sub PublicFoo() ' Noncompliant {{Make sure that exposing this HTTP endpoint is safe here.}}
'              ^^^^^^^^^
    End Sub

    Protected Sub ProtectedFoo()
    End Sub

    Friend Sub InternalFoo()
    End Sub

    Private Sub PrivateFoo()
    End Sub

    Private Class Bar
        Inherits Controller

        Public Sub InnerFoo()
        End Sub
    End Class
End Class
