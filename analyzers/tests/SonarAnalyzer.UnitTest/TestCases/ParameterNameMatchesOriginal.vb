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

Public Interface IGenericInterface(Of A)
    Sub DoSomething(ByVal value As A)
    Sub DoSomething(ByVal value As A, intValue As Integer)
    Sub DoSomethingElse(ByVal value As A)
    Sub DoSomethingElse(ByVal value As A, intValue As Integer)
    Sub TryOneMoreTime(ByVal value As AnotherParameterClass)

End Interface

Public Class ParameterClass
End Class

Public Class AnotherParameterClass
End Class

Public Class Implementation
    Implements IGenericInterface(Of ParameterClass)
    Public Sub DoSomething(parameter As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomething
    End Sub
    Public Sub DoSomething(value As ParameterClass, intValue As Integer) Implements IGenericInterface(Of ParameterClass).DoSomething
    End Sub
    Public Sub DoSomethingElse(myValue As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomethingElse                      ' Noncompliant
        '                      ^^^^^^^
    End Sub
    Public Sub DoSomethingElse(value As ParameterClass, intVal As Integer) Implements IGenericInterface(Of ParameterClass).DoSomethingElse     ' Noncompliant
        '                                               ^^^^^^
    End Sub
    Public Sub TryOneMoreTime(anotherParameter As AnotherParameterClass) Implements IGenericInterface(Of ParameterClass).TryOneMoreTime        ' Noncompliant
        '                     ^^^^^^^^^^^^^^^^
    End Sub
End Class
