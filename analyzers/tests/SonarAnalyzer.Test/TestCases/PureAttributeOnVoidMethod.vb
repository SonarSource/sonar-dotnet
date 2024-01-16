Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Diagnostics.Contracts
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Runtime.InteropServices

Namespace Tests.TestCases
    Public Class MyAttribute
        Inherits Attribute
    End Class

    Class Person
        Private age As Integer

        <Pure> ' Noncompliant {{Remove the 'Pure' attribute or change the method to return a value.}}
        Private Sub ConfigureAge(ByVal age As Integer)
'        ^^^^ @-1
            Me.age = age
        End Sub

        <Pure> 'Noncompliant
        Sub WithExplicitInParameter(<[In]> ByVal age As Integer)
            Me.age = age
        End Sub

        <Pure> 'Noncompliant
        Function TaskDoesNotRetrun(input As Integer) As Task
            Return Task.FromResult(input)
        End Function

        <Pure> 'Noncompliant
        Async Function AsyncTaskDoesNotRetrun(input As Integer) As Task
            Me.age = input
        End Function

        <My>
        Private Sub ConfigureAge2(ByVal age As Integer)
            Me.age = age
        End Sub

        <Pure>
        Private Function ConfigureAge3(ByVal age As Integer) As Integer
            Return age
        End Function

        <Pure>
        Private Sub ConfigureAge4(ByVal age As Integer, <Out> ByRef ret As Integer)
            ret = age
        End Sub

        <Pure>
        Private Sub SubWithByref(ByRef age As Integer)
            age += 1
        End Sub

        <Pure>
        Function TaskOfTReturns(input As Integer) As Task(Of Integer)
            Return Task.FromResult(input * 42)
        End Function

        <Pure>
        Function TaskWithOutParameter(input As Integer, <Out> ByRef ret As Integer) As Task
            ret = input
            Return Task.FromResult(input)
        End Function
    End Class
End Namespace
