Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class Program
        Public Sub Method_01(arg1 As Integer, argument As Integer)
            If arg1 < 0 Then
                Throw New Exception("arg1") ' Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}
'                                   ^^^^^^
            End If

            If ARG1 < 0 Then
                Throw New ArgumentException("ARG1") ' Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}
            End If

            Const foo As String = "arg1"

            ValidateArgument(arg1, "arg1")

            If "arg1" = foo Then
                Return
            End If

            Throw New ArgumentException("argument ") ' Noncompliant
            Throw New ArgumentException("argument,") ' Noncompliant
            Throw New ArgumentException("This is argument.") ' Noncompliant
'                                       ^^^^^^^^^^^^^^^^^^^
            Throw New ArgumentException("argument!") ' Noncompliant
            Throw New ArgumentException("argument?") ' Noncompliant
            Throw New ArgumentException("argument and arg2") ' Noncompliant
            Throw New ArgumentException("ARGUMENT") ' Noncompliant

            Throw New ArgumentException("arg ") ' too short name
            Throw New ArgumentException("argument123")
            Throw New ArgumentException("arg123")
        End Sub

        Private Sub ValidateArgument(v As Integer, name As String)
            If v < 0 Then
                Throw New ArgumentOutOfRangeException("name") ' Noncompliant
            End If

            If v < 0 Then
                Throw New Exception(NameOf(name))
            End If

            If v < 0 Then
                Throw New Exception($"{NameOf(name)} is not valid with value {name}")
            End If

        End Sub

        Public Sub New(arg1 As String, argument As Integer, anotherArgument As String)
            If arg1 is Nothing Or arg1.Length = 0 Then
                Throw New Exception($"The arg1 with value {arg1} is not valid") ' too short
            End If

            Throw New Exception($"argument with value {argument} is not valid") ' Noncompliant
'                                 ^^^^^^^^^^^^^^^^^^^^
            Throw New Exception($"arg1") ' Noncompliant

            Throw New Exception("anotherArgument argument argument value ""{argument}"" is not valid") ' Noncompliant
            Throw New Exception("argument anotherArgument argument anotherArgument") ' Noncompliant
        End Sub

        Public Function Method_03(arg1 As Integer, arg2 As Integer) As Boolean
            Throw New Exception("arg2") ' Noncompliant
        End Function

        Public Function SameStringTokens(argumentName As Integer) As Boolean
            Throw New ArgumentException("argumentName", "argumentName") ' Noncompliant
            ' Noncompliant@-1
            Throw New Exception("argumentName") ' Noncompliant
            Throw New ArgumentException("argumentName argumentName argumentName") ' Noncompliant (only one message)
            Throw New ArgumentException("argumentName argumentName argumentName") ' Noncompliant
            Throw New Exception($"argumentName argumentName argumentName") ' Noncompliant (only one message)
            Throw New Exception($"argumentName argumentName argumentName") ' Noncompliant
            Throw New Exception("argumentName") ' Noncompliant
            Throw New Exception("argumentName") ' Noncompliant
        End Function
    End Class
End Namespace
