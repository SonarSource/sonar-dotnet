Public Class FooBase
    Public Overridable Sub Method()
    End Sub
End Class

Public Class FooImpl
    Inherits FooBase

    Public Overrides Sub Method() ' Noncompliant
    End Sub
End Class
