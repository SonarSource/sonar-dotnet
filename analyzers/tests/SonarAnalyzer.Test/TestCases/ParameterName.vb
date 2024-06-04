Module Module1

    Private WithEvents fSource As Source

    Sub GetSomething(ByVal ID As Integer) ' Noncompliant
        '                  ^^
    End Sub

    Sub GetSomething2(ByVal id As Integer, Name As String) ' Noncompliant
        '                                  ^^^^
    End Sub

    Dim array As String()

    ReadOnly Property Foo(ByVal Index As Integer)  ' Noncompliant
        '                       ^^^^^
        Get
            Return array(Index)
        End Get
    End Property

    Private Sub fSource_SomeEvent(Sender As Object) Handles fSource.SomeEvent ' Noncompliant FP
    End Sub

End Module

Public Class Source

    Public Event SomeEvent(Sender As Object) ' Noncompliant

End Class

Public Interface ISomething

    Sub DoSomething(Name As String)             ' Noncompliant
    ReadOnly Property Value(Index As Integer)   ' Noncompliant

End Interface

Public Class Something
    Implements ISomething

    Public ReadOnly Property Value(Index As Integer) As System.Object Implements ISomething.Value  ' Noncompliant FP
        Get
            Return 42
        End Get
    End Property

    Public Sub DoSomething(Name As String) Implements ISomething.DoSomething     ' Noncompliant FP
    End Sub

End Class

Public Class Base

    Protected Overridable Sub DoSomething(Name As String) ' Noncompliant
    End Sub

End Class

Public Class Derived
    Inherits Base

    Protected Overrides Sub DoSomething(Name As String) ' Noncompliant FP
    End Sub

End Class
