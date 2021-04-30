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
    Sub DoSomething()
    Sub DoSomething(ByVal value As A)
    Sub DoSomething(ByVal value As A, ByVal intValue As Integer)
    Sub DoSomethingElse(ByVal value As A)
    Sub DoSomethingElse(ByVal value As A, ByVal parameterClassValue As ParameterClass)
    Sub TryOneMoreTime(ByVal value As AnotherParameterClass)
    Sub DoSomethingCaseSensitive(ByVal value As A, ByVal intValue As Integer)
End Interface

Public Class ParameterClass
End Class

Public Class AnotherParameterClass
End Class

Public Class Implementation
    Implements IGenericInterface(Of ParameterClass)
    Public Sub DoSomething() Implements IGenericInterface(Of ParameterClass).DoSomething
    End Sub
    Public Sub DoSomething(ByVal parameter As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomething
    End Sub
    Public Sub DoSomething(ByVal randomName As AnotherParameterClass)
    End Sub
    Public Sub DoSomethingElse(ByVal completelyAnotherName As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomethingElse
    End Sub
    Public Sub DoSomething(ByVal value As ParameterClass, ByVal myValue As Integer) Implements IGenericInterface(Of ParameterClass).DoSomething                             ' Noncompliant
        '                                                       ^^^^^^^
    End Sub
    Public Sub DoSomethingElse(ByVal value As ParameterClass, ByVal val As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomethingElse                  ' Noncompliant
        '                                                           ^^^
    End Sub
    Public Sub TryOneMoreTime(ByVal anotherParameter As AnotherParameterClass) Implements IGenericInterface(Of ParameterClass).TryOneMoreTime                               ' Noncompliant
        '                           ^^^^^^^^^^^^^^^^
    End Sub
    Public Sub DoSomethingCaseSensitive(ByVal Value As ParameterClass, ByVal IntValue As Integer) Implements IGenericInterface(Of ParameterClass).DoSomethingCaseSensitive
    End Sub
End Class

Public Structure StructImplementation
    Implements IGenericInterface(Of ParameterClass)
    Public Sub DoSomething() Implements IGenericInterface(Of ParameterClass).DoSomething
    End Sub
    Public Sub DoSomething(ByVal parameter As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomething
    End Sub
    Public Sub DoSomething(ByVal randomName As AnotherParameterClass)
    End Sub
    Public Sub DoSomethingElse(ByVal completelyAnotherName As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomethingElse
    End Sub
    Public Sub DoSomething(ByVal value As ParameterClass, ByVal myValue As Integer) Implements IGenericInterface(Of ParameterClass).DoSomething                             ' Noncompliant
        '                                                       ^^^^^^^
    End Sub
    Public Sub DoSomethingElse(ByVal value As ParameterClass, ByVal val As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomethingElse                  ' Noncompliant
        '                                                           ^^^
    End Sub
    Public Sub TryOneMoreTime(ByVal anotherParameter As AnotherParameterClass) Implements IGenericInterface(Of ParameterClass).TryOneMoreTime                               ' Noncompliant
        '                           ^^^^^^^^^^^^^^^^
    End Sub
    Public Sub DoSomethingCaseSensitive(ByVal Value As ParameterClass, ByVal IntValue As Integer) Implements IGenericInterface(Of ParameterClass).DoSomethingCaseSensitive
    End Sub
End Structure

Public MustInherit Class BaseClass(Of T)
    Public MustOverride Sub SomeMethod(ByVal someParameter As T)
    Public MustOverride Sub SomeMethod(ByVal someParameter As T, ByVal intParam As Integer)
End Class

Public Class ClassOne
    Inherits BaseClass(Of Integer)
    Public Overrides Overloads Sub SomeMethod(ByVal renamedParam As Integer)
    End Sub
    Public Overrides Overloads Sub SomeMethod(ByVal someParameter As Integer, ByVal renamedParam As Integer)  ' Noncompliant
        '                                                                           ^^^^^^^^^^^^
    End Sub
End Class

Public MustInherit Class AbstractClassWithGenericMethod
    MustOverride Public Sub Foo(Of T)(ByVal val As T)
    MustOverride Public Sub Bar(Of T)(ByVal val As T)
End Class

Public Class InheritedClassWithDefinition
    Inherits AbstractClassWithGenericMethod

    Public Overrides Sub Foo(Of T)(ByVal myNewName As T)  ' Noncompliant
        '                                ^^^^^^^^^
    End Sub
    Public Overrides Sub Bar(Of T)(ByVal val As T)
    End Sub
End Class

Public Interface IAnotherGenericInterface(Of A)
    Sub DoSomething(ByVal value As A)
    Sub DoSomething(ByVal value As A, ByVal intValue As Integer)
End Interface

Public Interface IAnotherInterface
    Inherits IAnotherGenericInterface(Of ParameterClass)
    Sub DoSomethingElse(ByVal value As ParameterClass)
End Interface

Public MustInherit Class AnotherAbstractClass
    Implements IAnotherInterface
    Public MustOverride Sub DoSomething(ByVal abstractValue As ParameterClass) Implements IAnotherGenericInterface(Of ParameterClass).DoSomething
    Public MustOverride Sub DoSomething(ByVal value As ParameterClass, ByVal IntValue As Integer) Implements IAnotherGenericInterface(Of ParameterClass).DoSomething
    Public MustOverride Sub DoSomethingElse(ByVal Value As ParameterClass) Implements IAnotherInterface.DoSomethingElse
End Class

Public Class AnotherImplementation
    Inherits AnotherAbstractClass

    Public Overrides Overloads Sub DoSomething(ByVal value As ParameterClass)  'Noncompliant {{Rename parameter 'value' to 'abstractValue' to match the base class declaration.}}
        '                                            ^^^^^
    End Sub
    Public Overrides Overloads Sub DoSomething(ByVal abstractValue As ParameterClass, ByVal IntValue As Integer)  'Noncompliant {{Rename parameter 'abstractValue' to 'value' to match the base class declaration.}}
        '                                            ^^^^^^^^^^^^^
    End Sub
    Public Overrides Sub DoSomethingElse(ByVal Value As ParameterClass)
    End Sub
End Class

