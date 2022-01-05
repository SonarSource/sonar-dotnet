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
    Sub DoSomething(Value As A)
    Sub DoSomething(Value As A, IntValue As Integer)
    Sub DoSomethingElse(Value As A)
    Sub DoSomethingElse(Value As A, ParameterClassValue As ParameterClass)
    Sub TryOneMoreTime(Value As AnotherParameterClass)
    Sub DoSomethingCaseSensitive(Value As A, IntValue As Integer)

End Interface

Public Class ParameterClass
End Class

Public Class AnotherParameterClass
End Class

Public Class Implementation
    Implements IGenericInterface(Of ParameterClass)

    Public Sub DoSomething() Implements IGenericInterface(Of ParameterClass).DoSomething
    End Sub

    Public Sub DoSomething(Parameter As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomething
    End Sub

    Public Sub DoSomething(RandomName As AnotherParameterClass)
    End Sub

    Public Sub DoSomethingElse(CompletelyAnotherName As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomethingElse
    End Sub

    Public Sub DoSomething(Value As ParameterClass, MyValue As Integer) Implements IGenericInterface(Of ParameterClass).DoSomething                             ' Noncompliant
        '                                           ^^^^^^^
    End Sub

    Public Sub DoSomethingElse(Value As ParameterClass, Val As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomethingElse                  ' Noncompliant
        '                                               ^^^
    End Sub

    Public Sub TryOneMoreTime(AnotherParameter As AnotherParameterClass) Implements IGenericInterface(Of ParameterClass).TryOneMoreTime                               ' Noncompliant
        '                     ^^^^^^^^^^^^^^^^
    End Sub

    Public Sub DoSomethingCaseSensitive(VALUE As ParameterClass, INTVALUE As Integer) Implements IGenericInterface(Of ParameterClass).DoSomethingCaseSensitive
    End Sub

End Class

Public Structure StructImplementation
    Implements IGenericInterface(Of ParameterClass)

    Public Sub DoSomething() Implements IGenericInterface(Of ParameterClass).DoSomething
    End Sub

    Public Sub DoSomething(Parameter As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomething
    End Sub

    Public Sub DoSomething(RandomName As AnotherParameterClass)
    End Sub

    Public Sub DoSomethingElse(CompletelyAnotherName As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomethingElse
    End Sub

    Public Sub DoSomething(Value As ParameterClass, MyValue As Integer) Implements IGenericInterface(Of ParameterClass).DoSomething                             ' Noncompliant
        '                                           ^^^^^^^
    End Sub

    Public Sub DoSomethingElse(Value As ParameterClass, Val As ParameterClass) Implements IGenericInterface(Of ParameterClass).DoSomethingElse                  ' Noncompliant
        '                                               ^^^
    End Sub

    Public Sub TryOneMoreTime(AnotherParameter As AnotherParameterClass) Implements IGenericInterface(Of ParameterClass).TryOneMoreTime                               ' Noncompliant
        '                     ^^^^^^^^^^^^^^^^
    End Sub

    Public Sub DoSomethingCaseSensitive(VALUE As ParameterClass, INTVALUE As Integer) Implements IGenericInterface(Of ParameterClass).DoSomethingCaseSensitive
    End Sub

End Structure

Public MustInherit Class BaseClass(Of T)

    Public MustOverride Sub SomeMethod(SomeParameter As T)
    Public MustOverride Sub SomeMethod(SomeParameter As T, IntParam As Integer)

End Class

Public Class ClassOne
    Inherits BaseClass(Of Integer)

    Public Overrides Overloads Sub SomeMethod(RenamedParam As Integer)
    End Sub

    Public Overrides Overloads Sub SomeMethod(SomeParameter As Integer, RenamedParam As Integer)  ' Noncompliant
        '                                                               ^^^^^^^^^^^^
    End Sub

End Class

Public MustInherit Class AbstractClassWithGenericMethod

    MustOverride Public Sub Foo(Of T)(Val As T)
    MustOverride Public Sub Bar(Of T)(Val As T)

End Class

Public Class InheritedClassWithDefinition
    Inherits AbstractClassWithGenericMethod

    Public Overrides Sub Foo(Of T)(MyNewName As T)  ' Noncompliant
        '                          ^^^^^^^^^
    End Sub

    Public Overrides Sub Bar(Of T)(Val As T)
    End Sub

End Class

Public Interface IAnotherGenericInterface(Of A)

    Sub DoSomething(Value As A)
    Sub DoSomething(Value As A, IntValue As Integer)

End Interface

Public Interface IAnotherInterface
    Inherits IAnotherGenericInterface(Of ParameterClass)

    Sub DoSomethingElse(Value As ParameterClass)

End Interface

Public MustInherit Class AnotherAbstractClass
    Implements IAnotherInterface
    Public MustOverride Sub DoSomething(AbstractValue As ParameterClass) Implements IAnotherGenericInterface(Of ParameterClass).DoSomething
    Public MustOverride Sub DoSomething(Value As ParameterClass, IntValue As Integer) Implements IAnotherGenericInterface(Of ParameterClass).DoSomething
    Public MustOverride Sub DoSomethingElse(Value As ParameterClass) Implements IAnotherInterface.DoSomethingElse

End Class

Public Class AnotherImplementation
    Inherits AnotherAbstractClass

    Public Overrides Overloads Sub DoSomething(Value As ParameterClass)  'Noncompliant {{Rename parameter 'Value' to 'AbstractValue' to match the base class declaration.}}
        '                                      ^^^^^
    End Sub

    Public Overrides Overloads Sub DoSomething(AbstractValue As ParameterClass, IntValue As Integer)  'Noncompliant {{Rename parameter 'AbstractValue' to 'Value' to match the base class declaration.}}
        '                                      ^^^^^^^^^^^^^
    End Sub

    Public Overrides Sub DoSomethingElse(Value As ParameterClass)
    End Sub

End Class

' See https://github.com/SonarSource/sonar-dotnet/issues/4370
Public Interface SomeInterface(Of A)
End Interface

Public Interface BaseInterface(Of A)
    Sub Apply(ByVal param As SomeInterface(Of A))
End Interface

Public Class BasicImplementation
    Implements BaseInterface(Of Integer)

    Public Sub Apply(ByVal intValueParam As SomeInterface(Of Integer)) Implements BaseInterface(Of Integer).Apply
    End Sub
End Class

Public Class StillGeneric(Of T)
    Implements BaseInterface(Of T)

    Public Sub Apply(ByVal renamedParam As SomeInterface(Of T)) Implements BaseInterface(Of T).Apply  ' Noncompliant
    End Sub
End Class

Public MustInherit Class AbstractClass(Of T)
    Public MustOverride Sub Apply(ByVal param As SomeInterface(Of T))
End Class

Public Class OverridenCompliant
    Inherits AbstractClass(Of Integer)

    Public Overrides Overloads Sub Apply(ByVal intValueParam As SomeInterface(Of Integer))
    End Sub
End Class

Public Class OverridenNonCompliant(Of K)
    Inherits AbstractClass(Of K)

    Public Overrides Overloads Sub Apply(ByVal renamedParam As SomeInterface(Of K)) ' Noncompliant
    End Sub
End Class
