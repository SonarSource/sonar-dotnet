' Commented line for concurrent namespace
Imports System.Diagnostics.CodeAnalysis

<ExcludeFromCodeCoverage> ' Noncompliant^2#23 {{Add a justification.}}
Class Noncompliant
    <ExcludeFromCodeCoverage()> Private Sub WithBrackets() ' Noncompliant^6#25
    End Sub

    <System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage> ' Noncompliant
    Private Sub FullyDeclaredNamespace()
    End Sub

    <Global.System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage> ' Noncompliant
    Private Sub GloballyDeclaredNamespace()
    End Sub

    <ExcludeFromCodeCoverage> ' Noncompliant
    <CLSCompliant(False)>
    Private Function Multiple() As UInteger
        Return 0
    End Function

    <ExcludeFromCodeCoverage, CLSCompliant(False)> Private Function Combined() As UInteger ' Noncompliant
        Return 0
    End Function

    <ExcludeFromCodeCoverage> ' Noncompliant
    Private Sub New()
    End Sub

    <ExcludeFromCodeCoverage> ' Noncompliant
    Private Sub Method()
    End Sub

    <ExcludeFromCodeCoverage> ' Noncompliant
    Private Property [Property] As Integer

    <ExcludeFromCodeCoverage> ' Noncompliant
    Private Event [Event] As EventHandler

End Class

Interface IInterface
    <ExcludeFromCodeCoverage> ' Noncompliant
    Sub Method()
End Interface

<ExcludeFromCodeCoverage> ' Noncompliant
Structure ProgramStruct
    <ExcludeFromCodeCoverage> ' Noncompliant
    Private Sub Method()
    End Sub
End Structure

<ExcludeFromCodeCoverage(Justification:="justification")>
Class Compliant

    <ExcludeFromCodeCoverage(Justification:="justification")>
    Private Sub New()
    End Sub

    <ExcludeFromCodeCoverage(Justification:="justification")>
    Private Sub Method()
    End Sub

    <ExcludeFromCodeCoverage(Justification:="justification")>
    Private Property [Property] As String

    <ExcludeFromCodeCoverage(Justification:="justification")>
    Private Event [Event] As EventHandler
End Class

Interface IComplaintInterface
    <ExcludeFromCodeCoverage(Justification:="justification")>
    Sub Method()
End Interface

<ExcludeFromCodeCoverage(Justification:="justification")>
Structure ComplaintStruct
    <ExcludeFromCodeCoverage(Justification:="justification")>
    Private Sub Method()
    End Sub
End Structure

Class NotApplicable

    Private Sub New()
    End Sub

    Private Sub Method()
    End Sub

    Private Property [Property] As Integer

    Private Event [Event] As EventHandler

    <NotSystem.ExcludeFromCodeCoverageAttribute>
    Private Sub SameName()
    End Sub
End Class

Namespace NotSystem
    Public Class ExcludeFromCodeCoverageAttribute
        Inherits Attribute
    End Class
End Namespace
