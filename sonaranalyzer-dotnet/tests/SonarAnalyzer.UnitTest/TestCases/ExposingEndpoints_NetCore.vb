Imports Microsoft.AspNetCore.Mvc

Public Class SomeController
    Inherits ControllerBase

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
        Inherits ControllerBase

        Public Sub InnerFoo()
        End Sub
    End Class
End Class

<Controller>
Public Class MyController
    Public Sub PublicFoo() ' Noncompliant
    End Sub
End Class

<NonController>
Public Class MyNoncontroller
    Inherits ControllerBase

    Public Sub PublicFoo() ' Compliant, not a controller
    End Sub
End Class
