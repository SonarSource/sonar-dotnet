Imports System.Web.Mvc

Public Class Foo
    Inherits Controller

    Public sub New () ' Compliant, ctors should not be highlighted
    End sub

    Protected Overrides Sub Finalize() ' Compliant, desctructors should not be highlighted; in addition, this protected and will not be highlighted anyway
    End Sub

    Public Property MyProperty As Integer  ' Compliant, properties should not be highlighted
        Get
            Return 0
        End Get
        Set(ByVal value As Integer)
        End Set
    End Property

    Public Sub PublicFoo() ' Noncompliant {{Make sure that exposing this HTTP endpoint is safe here.}}
'              ^^^^^^^^^
    End Sub

    Protected Sub ProtectedFoo()
    End Sub

    Friend Sub InternalFoo()
    End Sub

    Private Sub PrivateFoo()
    End Sub

    <NonAction>
    Public Sub PublicNonAction() ' Compliant, methods decorated with NonAction are not entrypoints
    End Sub

    Private Class Bar
        Inherits Controller

        Public Sub InnerFoo()
        End Sub
    End Class
End Class
