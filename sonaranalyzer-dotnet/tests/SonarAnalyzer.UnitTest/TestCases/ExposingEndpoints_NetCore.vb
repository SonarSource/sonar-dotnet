Imports Microsoft.AspNetCore.Mvc

Public Class SomeController
    Inherits ControllerBase

    Public Sub New() ' Compliant, ctors should not be highlighted
    End sub

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
        Inherits ControllerBase

        Public Sub InnerFoo()
        End Sub
    End Class
End Class

<Controller>
Public Class MyController
    Public Sub New() ' Compliant, ctor should not be highlighted
    End sub

    Public Sub PublicFoo() ' Noncompliant
    End Sub
End Class

<NonController>
Public Class MyNoncontroller
    Inherits ControllerBase

    Public Sub PublicFoo() ' Compliant, not a controller
    End Sub
End Class
