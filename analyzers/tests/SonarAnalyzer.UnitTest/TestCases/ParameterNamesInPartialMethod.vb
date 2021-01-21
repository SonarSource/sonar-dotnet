'Partial methods are not relevant for VB.NET

Public MustInherit Class Base

    Public MustOverride Sub DoSomethingMustOverride(Name As String)

    Public Overridable Sub DoSomethingOverridable(Name As String)
    End Sub

    Public Sub NoParameters()
    End Sub

    Public Sub NoParametersNoParenthesis  'This should not have ()
    End Sub

End Class

Public Interface IContract

    Sub Add(Count As Integer)

End Interface

Public Class GoodUsage
    Inherits Base
    Implements IContract

    Public Overrides Sub DoSomethingMustOverride(Name As String)
    End Sub

    Public Overrides Sub DoSomethingOverridable(Name As String)
    End Sub

    Public Sub Add(Count As Integer) Implements IContract.Add
    End Sub

End Class

Public Class BadUsage
    Inherits Base
    Implements IContract

    Public Overrides Sub DoSomethingMustOverride(Description As String) ' Noncompliant {{Rename parameter 'Description' to 'Name' to match the base class declaration.}}
        '                                        ^^^^^^^^^^^
    End Sub

    Public Overrides Sub DoSomethingOverridable(Description As String)  ' Noncompliant
    End Sub

    Public Sub Add(Difference As Integer) Implements IContract.Add      ' Noncompliant {{Rename parameter 'Difference' to 'Count' to match the interface declaration.}}
        '          ^^^^^^^^^^
    End Sub

End Class

Public Class IgnoreCaseChange
    Implements IContract

    Public Sub Add(count As Integer) Implements IContract.Add   ' Compliant, differs only in casing
    End Sub

End Class
