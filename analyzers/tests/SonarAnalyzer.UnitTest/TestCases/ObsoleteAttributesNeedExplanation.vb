' Commented line for concurrent namespace

<Obsolete>  ' Noncompliant^2#8 {{Add an explanation.}}
Class Noncompliant
    <Obsolete()> Private Sub WithBrackets() ' Noncompliant {{Add an explanation.}}
'    ^^^^^^^^^^
    End Sub

    <System.Obsolete> ' Noncompliant
    Private Sub FullyDeclaredNamespace()
    End Sub

    <Global.System.Obsolete> ' Noncompliant
    Private Sub GloballyDeclaredNamespace()
    End Sub

    <Obsolete(Nothing)> ' Noncompliant
    Sub WithNothing() { }
    End Sub

    <Obsolete("")> ' Noncompliant
    Sub WithEmptyString() { }
    End Sub

    <Obsolete("  ")> ' Noncompliant
    Sub WithWhiteSpace() { }
    End Sub

    <Obsolete> ' Noncompliant
    <CLSCompliant(False)>
    Private Function Multiple() As UInteger
        Return 0
    End Function

    <Obsolete, CLSCompliant(False)> Private Function Combined() As UInteger ' Noncompliant
        Return 0
    End Function

    <Obsolete> ' Noncompliant
    Enum [Enum]
        foo
        bar
    End Enum

    <Obsolete> ' Noncompliant
    Private Sub New()
    End Sub

    <Obsolete> ' Noncompliant
    Private Sub Method()
    End Sub

    <Obsolete> ' Noncompliant
    Private Property [Property] As Integer
    <Obsolete> ' Noncompliant
    Private Field As Integer
    <Obsolete> ' Noncompliant
    Private Event [Event] As EventHandler
    <Obsolete> ' Noncompliant
    Delegate Sub [Delegate]()
End Class

<Obsolete> ' Noncompliant
Interface IInterface
    <Obsolete> ' Noncompliant
    Sub Method()
End Interface

<Obsolete> ' Noncompliant
Structure ProgramStruct
    <Obsolete> ' Noncompliant
    Private Sub Method()
    End Sub
End Structure

<Obsolete("explanation")>
Class Compliant
    <Obsolete("explanation")>
    Enum [Enum]
        foo
        bar
    End Enum

    <Obsolete("explanation")>
    Private Sub New()
    End Sub

    <Obsolete("explanation")>
    Private Sub Method()
    End Sub

    <Obsolete("explanation")>
    Private Property [Property] As String
    <Obsolete("explanation", True)>
    Private Field As Integer
    <Obsolete("explanation", False)>
    Private Event [Event] As EventHandler
    <Obsolete("explanation")>
    Delegate Sub [Delegate]()
End Class

<Obsolete("explanation")>
Interface IComplaintInterface
    <Obsolete("explanation")>
    Sub Method()
End Interface

<Obsolete("explanation")>
Structure ComplaintStruct
    <Obsolete("explanation")>
    Private Sub Method()
    End Sub
End Structure

Class NotApplicable
    <CLSCompliant(False)>
    Enum [Enum]
        foo
        bar
    End Enum

    Private Sub New()
    End Sub

    Private Sub Method()
    End Sub

    Private Property [Property] As Integer
    Private Field As Integer
    Private Event [Event] As EventHandler
    Delegate Sub [Delegate]()

    <NotSystem.ObsoleteAttribute>
    Private Sub SameName()
    End Sub
End Class

Namespace NotSystem
    Public Class ObsoleteAttribute
        Inherits Attribute
    End Class
End Namespace
