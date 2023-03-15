' Commented line for concurrent namespace
Imports System.Diagnostics.CodeAnalysis
Imports [Alias] = System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute

<ExcludeFromCodeCoverage> ' Noncompliant^2#23 {{Add a justification.}}
Class Noncompliant
    <ExcludeFromCodeCoverage()> Sub WithBrackets() ' Noncompliant^6#25
    End Sub

    <System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage> ' Noncompliant
    Sub FullyDeclaredNamespace()
    End Sub

    <Global.System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage> ' Noncompliant
    Sub GloballyDeclaredNamespace()
    End Sub

    <[Alias](Justification:="")> ' Noncompliant
    Sub WithAlias()
    End Sub

    <ExcludeFromCodeCoverage(Justification:=Nothing)> ' Noncompliant
    Sub WithNothing()
    End Sub

    <ExcludeFromCodeCoverage(Justification:="")> ' Noncompliant
    Sub WithEmptyString()
    End Sub

    <ExcludeFromCodeCoverage(Justification:="  ")> ' Noncompliant
    Sub WithWhiteSpace()
    End Sub

    <ExcludeFromCodeCoverage> ' Noncompliant
    <CLSCompliant(False)>
    Function Multiple() As UInteger
        Return 0
    End Function

    <ExcludeFromCodeCoverage, CLSCompliant(False)> Function Combined() As UInteger ' Noncompliant
        Return 0
    End Function

    <ExcludeFromCodeCoverage> ' Noncompliant
    Sub New()
    End Sub

    <ExcludeFromCodeCoverage> ' Noncompliant
    Sub Method()
    End Sub

    <ExcludeFromCodeCoverage> ' Noncompliant
    Property [Property] As Integer

    <ExcludeFromCodeCoverage> ' Noncompliant
    Event [Event] As EventHandler

End Class

Interface IInterface
    <ExcludeFromCodeCoverage> ' Noncompliant
    Sub Method()
End Interface

<ExcludeFromCodeCoverage> ' Noncompliant
Structure ProgramStruct
    <ExcludeFromCodeCoverage> ' Noncompliant
    Sub Method()
    End Sub
End Structure

<ExcludeFromCodeCoverage(Justification:="justification")>
Class Compliant

    <ExcludeFromCodeCoverage(Justification:="justification")>
    Sub New()
    End Sub

    <ExcludeFromCodeCoverage(Justification:="justification")>
    Sub Method()
    End Sub

    <ExcludeFromCodeCoverage(Justification:="justification")>
    Property [Property] As String

    <ExcludeFromCodeCoverage(Justification:="justification")>
    Event [Event] As EventHandler
End Class

Interface IComplaintInterface
    <ExcludeFromCodeCoverage(Justification:="justification")>
    Sub Method()
End Interface

<ExcludeFromCodeCoverage(Justification:="justification")>
Structure ComplaintStruct
    <ExcludeFromCodeCoverage(Justification:="justification")>
    Sub Method()
    End Sub
End Structure

Class NotApplicable

    Sub New()
    End Sub

    Sub Method()
    End Sub

    Property [Property] As Integer

    Event [Event] As EventHandler

    <NotSystem.ExcludeFromCodeCoverageAttribute>
    Sub SameName()
    End Sub
End Class

Namespace NotSystem
    Public Class ExcludeFromCodeCoverageAttribute
        Inherits Attribute
    End Class
End Namespace
