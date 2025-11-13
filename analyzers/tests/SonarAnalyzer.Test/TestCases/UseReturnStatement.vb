Imports System

Public Class ImplicitReturnStatementsAreNoncompliant
    Implements IInterface

    Public Function AssignedReturnValueOnly() As Integer
        AssignedReturnValueOnly = 42 ' Noncompliant ^9#23
    End Function

    Public Function AssignementStatements() As Integer
        AssignementStatements = 101 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        AssignementStatements += 42 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        AssignementStatements -= 17 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        AssignementStatements *= 99 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        AssignementStatements /= 24 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        AssignementStatements \= 21 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        AssignementStatements &= 42 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        AssignementStatements ^= 14 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        AssignementStatements <<= 2 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        AssignementStatements >>= 1 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
    End Function

    Public Function CaseInsensitive() As Integer
        CASEINSENSITIVE = 42  ' Noncompliant
    End Function

    Public Shared Function SharedFunction() As Integer
        SharedFunction = 42  ' Noncompliant
    End Function

    Public Function ExplictlyReturnDefaultReturnValue() As Integer
        Return ExplictlyReturnDefaultReturnValue ' Noncompliant {{Do not make use of the implicit return value.}}
    End Function

    Public Function ReadAssignementStatements() As Integer
        Dim value As Integer = ReadAssignementStatements ' Noncompliant  {{Do not make use of the implicit return value.}}
        value += ReadAssignementStatements  ' Noncompliant {{Do not make use of the implicit return value.}}
        value -= ReadAssignementStatements  ' Noncompliant {{Do not make use of the implicit return value.}}
        value *= ReadAssignementStatements  ' Noncompliant {{Do not make use of the implicit return value.}}
        value /= ReadAssignementStatements  ' Noncompliant {{Do not make use of the implicit return value.}}
        value \= ReadAssignementStatements  ' Noncompliant {{Do not make use of the implicit return value.}}
        value &= ReadAssignementStatements  ' Noncompliant {{Do not make use of the implicit return value.}}
        value ^= ReadAssignementStatements  ' Noncompliant {{Do not make use of the implicit return value.}}
        value <<= ReadAssignementStatements ' Noncompliant {{Do not make use of the implicit return value.}}
        value >>= ReadAssignementStatements ' Noncompliant {{Do not make use of the implicit return value.}}
        Return value
    End Function

    Public Function ImplementedInterfaceMethod() As Integer Implements IInterface.ImplementedInterfaceMethod
        ImplementedInterfaceMethod = 42 ' Noncompliant
    End Function

    Private Function ArgumentName() As Integer
        WithExplicitArgumentName(Something:=ArgumentName)    ' Noncompliant
    End Function

    Private Sub WithExplicitArgumentName(Something As Integer)
    End Sub

    ' https://github.com/SonarSource/sonar-dotnet/issues/4159
    Public Function Repro_4159() As String
        Repro_4159 = NameOf(Exception)  ' Noncompliant
        Return NameOf(Repro_4159)       ' Compliant
    End Function

End Class

Namespace NamespaceName

    Public Class Something
    End Class

End Namespace

Public Class Source

    Public Event SomeEvent()

End Class

Public Class DoesNotApplyOn
    Implements IInterface

    Private WithEvents fSource As Source

    Public Function FunctionWithExplictReturnOnly()
        Return 42 'Compliant
    End Function

    Public Sub SubMethods(number As Integer)
        Dim SubMethods = number ' Compliant
    End Sub

    Public Function RecursiveFunctionCalls(number As Integer)
        If number = 42 Then
            Return 42
        Else
            Return RecursiveFunctionCalls(42) ' Compliant, method call
        End If
    End Function

    Public Function CallToOtherProperty(other As OtherType) As Integer
        With other
            Return .CallToOtherProperty ' Compliant
        End With
    End Function

    Public Function CallToOtherFunction(other As OtherType) As Integer
        With other
            Return .CallToOtherFunction() ' Compliant
        End With
    End Function

    Public Function WriteToOtherProperty() As OtherType
        Return New OtherType With
        {
            .WriteToOtherProperty = 69 ' Compliant
        }
    End Function

    Public Function WriteToOtherFunction(other As OtherType) As Integer
        With other
            Return .WriteToOtherFunction(42) ' Compliant
        End With
    End Function

    Public Function ImplementedInterfaceMethod() As Integer Implements IInterface.ImplementedInterfaceMethod ' Compliant
        Return 42
    End Function

    <CustomAttribute>   ' Compliant
    Public Function CustomAttribute() As Integer
    End Function

    Private Function ArgumentName() As Integer
        WithExplicitArgumentName(ArgumentName:=42)  ' Compliant
    End Function

    Private Sub WithExplicitArgumentName(ArgumentName As Integer)
    End Sub

    ' https://github.com/SonarSource/sonar-dotnet/issues/4347
    Public Function OtherType() As OtherType    ' Compliant
        Dim Ret As OtherType                    ' Compliant
        Dim X As New OtherType                  ' Compliant
    End Function

    Public Function NamespaceName() As NamespaceName.Something  'Compliant
        Dim Ret As NamespaceName.Something                      'Compliant
    End Function

    Public Function Something() As NamespaceName.Something      'Compliant
        Dim Ret As NamespaceName.Something                      'Compliant
    End Function

    Public Sub SomeEvent() Handles fSource.SomeEvent
        Dim S As Source
        AddHandler S.SomeEvent, AddressOf SomeEvent
    End Sub

End Class

Public Class Repro_9553 ' https://github.com/SonarSource/sonar-dotnet/issues/9553
    Public Function TestFunc() As String
        Return Invoke(AddressOf TestFunc) 'Noncompliant FP
    End Function

    Private Function Invoke(func As Func(Of String)) As String
        Return func()
    End Function

End Class

Public Class OtherType
    Public Property CallToOtherProperty As Integer

    Public Property WriteToOtherProperty As Integer

    Public Function CallToOtherFunction() As Integer
        Return 42
    End Function

    Public Function WriteToOtherFunction(number As Integer) As Integer
        Return number
    End Function
End Class

Public Interface IInterface
    Function ImplementedInterfaceMethod() As Integer
End Interface

Public Class CustomAttribute
    Inherits Attribute

End Class

Namespace Repro_2559    'https://sonarsource.atlassian.net/browse/NET-2559

    Public Module Repro

        Public Function FunctionNameAndAlsoTypeName() As Integer
            Dim G As New Generic(Of FunctionNameAndAlsoTypeName)    ' Noncompliant FP
            GenericMethod(Of FunctionNameAndAlsoTypeName)()         ' Noncompliant FP
        End Function

        Public Sub GenericMethod(Of T)()
        End Sub

    End Module

    Public Class FunctionNameAndAlsoTypeName
    End Class

    Public Class Generic(Of T)
    End Class

End Namespace
