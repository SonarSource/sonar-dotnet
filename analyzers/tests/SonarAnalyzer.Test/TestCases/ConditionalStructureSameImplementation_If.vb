Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace Tests.TestCases
    Class ConditionalStructureSameCondition_If
        Function Test(someCondition1 As Boolean, someCondition2 As Boolean) As Object
            If (someCondition1) Then
                DoSomething1()
            Else
                DoSomething1() ' Compliant, single line implementation is ignored
            End If

            If someCondition1 Then DoSomething1() Else DoSomething1() ' Noncompliant
'                                                      ^^^^^^^^^^^^^^
            If someCondition1 Then DoSomething1() Else DoSomething2() : DoSomething1()

            If (someCondition1) Then
                DoSomething1()
                DoSomething1()
            ElseIf (someCondition2) Then
                DoSomething1()
                DoSomething2()
            ElseIf (someCondition2) Then
                DoSomething1() ' Noncompliant {{Either merge this branch with the identical one on line 24 or change one of the implementations.}}
                DoSomething2()
            ElseIf (someCondition2) Then
                DoSomething1() ' Noncompliant {{Either merge this branch with the identical one on line 24 or change one of the implementations.}}
                DoSomething2()
            Else
                DoSomething1() ' Noncompliant {{Either merge this branch with the identical one on line 21 or change one of the implementations.}}
                DoSomething1()
            End If

            If (someCondition1) Then
                DoSomething1()
            ElseIf (someCondition2) Then
                DoSomething1() ' Compliant, single line
            Else
                DoSomething1() ' Compliant, single line
            End If

            If (someCondition1) Then
                DoSomething2()
                Return DoSomething1()
            ElseIf (someCondition2) Then
                DoSomething2()
                Return DoSomething1() ' Compliant, single line
            Else
                DoSomething2()
                Return DoSomething1() ' Compliant, single line
            End If
        End Function

        Private Function DoSomething1
            Return Nothing
        End Function

        Private Function DoSomething2
            Return Nothing
        End Function

        Public Sub ExceptionOfException(a As Integer)
            If a = 1 Then
                DoSomething1()
            ElseIf a = 2 Then
                DoSomething1() ' Noncompliant
            End If
        End Sub

        Public Sub Exception(a As Integer)
            If a >= 0 AndAlso a < 10 Then
                DoSomething1()
            ElseIf a >= 10 AndAlso a < 20 Then
                DoSomething2()
            ElseIf a >= 20 AndAlso a < 50 ' Compliant
                DoSomething1()
            End If
        End Sub
    End Class
End Namespace

' https://github.com/SonarSource/sonar-dotnet/issues/9637
Public Class UnresolvedSymbols
    Public Function Method(first As String, second As String)
        If first.Length = 42 Then
            Dim ret = UnknownMethod()   ' Error [BC30451]
            Return ret
        ElseIf second.Length = 42 Then  ' Noncompliant@+1
            Dim ret = UnknownMethod()   ' Error [BC30451]
            Return ret
        End If
        Return ""
    End Function
End Class
