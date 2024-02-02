' Commented line for concurrent namespace
Imports System.Diagnostics.CodeAnalysis
Imports [Alias] = System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute

' Noncompliant@+1 ^2#23 {{Add a justification.}}
<ExcludeFromCodeCoverage>
Class Noncompliant
    <ExcludeFromCodeCoverage()> Sub WithBrackets() ' Noncompliant^6#25
    End Sub

    ' Noncompliant@+1
    <System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>
    Sub FullyDeclaredNamespace()
    End Sub

    ' Noncompliant@+1
    <Global.System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>
    Sub GloballyDeclaredNamespace()
    End Sub

    ' Noncompliant@+1
    <[Alias](Justification:="")>
    Sub WithAlias()
    End Sub

    ' Noncompliant@+1
    <ExcludeFromCodeCoverage(Justification:=Nothing)>
    Sub WithNothing()
    End Sub

    ' Noncompliant@+1
    <ExcludeFromCodeCoverage(Justification:="")>
    Sub WithEmptyString()
    End Sub

    ' Noncompliant@+1
    <ExcludeFromCodeCoverage(Justification:="  ")>
    Sub WithWhiteSpace()
    End Sub

    ' Noncompliant@+1
    <ExcludeFromCodeCoverage>
    <CLSCompliant(False)>
    Function Multiple() As UInteger
        Return 0
    End Function

    ' Noncompliant@+1
    <ExcludeFromCodeCoverage, CLSCompliant(False)> Function Combined() As UInteger
        Return 0
    End Function

    ' Noncompliant@+1
    <ExcludeFromCodeCoverage>
    Sub New()
    End Sub

    ' Noncompliant@+1
    <ExcludeFromCodeCoverage>
    Sub Method()
    End Sub

    ' Noncompliant@+1
    <ExcludeFromCodeCoverage>
    Property [Property] As Integer

    ' Noncompliant@+1
    <ExcludeFromCodeCoverage>
    Event [Event] As EventHandler

End Class

Interface IInterface
    ' Noncompliant@+1
    <ExcludeFromCodeCoverage>
    Sub Method()
End Interface

' Noncompliant@+1
<ExcludeFromCodeCoverage>
Structure ProgramStruct
    ' Noncompliant@+1
    <ExcludeFromCodeCoverage>
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
