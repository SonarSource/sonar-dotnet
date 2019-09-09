Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class Program
        Public Sub Method_01(arg1 As Integer, arg2 As Integer)
            If arg1 < 0 Then
                Throw New Exception("arg1") ' Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}
'                                   ^^^^^^
            End If

            If ARG1 < 0 Then
                Throw New ArgumentException("ARG1") ' Noncompliant {{Replace the string 'ARG1' with 'nameof(ARG1)'.}}
            End If

            Const foo As String = "arg1"

            ValidateArgument(arg1, "arg1")

            If "arg1" = foo Then
                Return
            End If

            Throw New ArgumentException("arg1 ")
            Throw New ArgumentException("arg123")
            Throw New ArgumentException(" ARG1")
        End Sub

        Private Sub ValidateArgument(v As Integer, name As String)
            If v < 0 Then
                Throw New ArgumentOutOfRangeException("name") ' Noncompliant
            End If

            If v < 0 Then
                Throw New Exception(NameOf(name))
            End If
        End Sub

        Public Sub New(arg1 As Integer, arg2 As Integer)
            If arg1 < 0 Then
                Throw New Exception("arg1") ' Noncompliant
            End If
        End Sub

        Public Function Method_03(arg1 As Integer, arg2 As Integer) As Boolean
            Throw New Exception("arg2") ' Noncompliant
        End Function
    End Class
End Namespace
