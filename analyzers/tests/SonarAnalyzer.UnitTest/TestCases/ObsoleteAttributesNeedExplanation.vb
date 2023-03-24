' Commented line for concurrent namespace

<Obsolete>  ' Noncompliant^2#8 {{Add an explanation.}}
Class Noncompliant
    <Obsolete()> Sub WithBrackets() ' Noncompliant {{Add an explanation.}}
'    ^^^^^^^^^^
    End Sub

    <System.Obsolete> ' Noncompliant
    Sub FullyDeclaredNamespace()
    End Sub

    <Global.System.Obsolete> ' Noncompliant
    Sub GloballyDeclaredNamespace()
    End Sub

    <Obsolete(Nothing)> ' Noncompliant
    Sub WithNothing()
    End Sub

    <Obsolete("")> ' Noncompliant
    Sub WithEmptyString()
    End Sub

    <Obsolete("  ")> ' Noncompliant
    Sub WithWhiteSpace()
    End Sub

    <Obsolete("", True)> ' Noncompliant
    Sub WithTwoArguments()
    End Sub

    <Obsolete> ' Noncompliant
    <CLSCompliant(False)>
    Function Multiple() As UInteger
        Return 0
    End Function

    <Obsolete, CLSCompliant(False)> Function Combined() As UInteger ' Noncompliant
        Return 0
    End Function

    <Obsolete> ' Noncompliant
    Enum [Enum]
        foo
        bar
    End Enum

    <Obsolete> ' Noncompliant
    Sub New()
    End Sub

    <Obsolete> ' Noncompliant
    Sub Method()
    End Sub

    <Obsolete> ' Noncompliant
    Property [Property] As Integer

    <Obsolete> ' Noncompliant
    Private Field As Integer

    <Obsolete> ' Noncompliant
    Event [Event] As EventHandler

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
    Sub Method()
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
    Sub New()
    End Sub

    <Obsolete("explanation")>
    Sub Method()
    End Sub

    <Obsolete("explanation", True)>
    Sub WithTwoArguments()
    End Sub

    <Obsolete("explanation")>
    Property [Property] As String

    <Obsolete("explanation", True)>
    Private Field As Integer

    <Obsolete("explanation", False)>
    Event [Event] As EventHandler

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
    Sub Method()
    End Sub
End Structure

Class NotApplicable
    <CLSCompliant(False)>
    Enum [Enum]
        foo
        bar
    End Enum

    Sub New()
    End Sub

    Sub Method()
    End Sub

    Property [Property] As Integer

    Private Field As Integer

    Event [Event] As EventHandler

    Delegate Sub [Delegate]()

    <NotSystem.ObsoleteAttribute>
    Sub SameName()
    End Sub
End Class

Namespace NotSystem
    Class ObsoleteAttribute
        Inherits Attribute
    End Class
End Namespace
