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

    Private Sub fSource_SomeEvent(Sender As Object) Handles fSource.SomeEvent
    End Sub

End Module

Public Class Source

    Public Event SomeEvent(Sender As Object) ' Noncompliant

End Class

Public Interface ISomething

    Function ReturnSomething(Name As String) As Integer ' Noncompliant
    Sub DoSomething(Name As String)                     ' Noncompliant
    ReadOnly Property Value(Index As Integer)           ' Noncompliant

End Interface

Public Class Something
    Implements ISomething

    Public Function ReturnSomething(Name As String) As Integer Implements ISomething.ReturnSomething
    End Function

    Public Sub DoSomething(Name As String) Implements ISomething.DoSomething
    End Sub

    Public ReadOnly Property Value(Index As Integer) As System.Object Implements ISomething.Value
        Get
            Return 42
        End Get
    End Property

End Class

Public Class Base

    Protected Overridable Sub DoSomething(Name As String) ' Noncompliant
    End Sub

    Protected Overridable Sub ToShadow(name As String)
    End Sub

    Public Overridable ReadOnly Property Value(Index As Integer) As Integer  ' Noncompliant
        Get
            Return 42
        End Get
    End Property

End Class

Public Class Derived
    Inherits Base

    Protected Overrides Sub DoSomething(Name As String)
    End Sub

    Protected Shadows Sub ToShadow(Name As String)  ' Noncompliant, because it's redefining the names
    End Sub

    Public Overrides ReadOnly Property Value(Index As Integer) As Integer  ' Compliant, follows base class
        Get
            Return 1024
        End Get
    End Property

End Class
