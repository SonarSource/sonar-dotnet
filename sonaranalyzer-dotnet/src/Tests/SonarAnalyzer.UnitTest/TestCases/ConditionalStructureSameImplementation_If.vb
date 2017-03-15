Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace Tests.TestCases
    Class ConditionalStructureSameCondition_If
        Sub Test()
            If (someCondition1) Then
                DoSomething1()
            Else
                DoSomething1() ' Noncompliant
            End If

            If someCondition Then DoSomething1() Else DoSomething1() End ' Noncompliant
'                                                     ^^^^^^^^^^^^^^
            If someCondition Then DoSomething1() Else DoSomething2() : DoSomething1() End

            If (someCondition1) Then
                DoSomething1()
                DoSomething1()
            ElseIf (someCondition2) Then

                DoSomething1()
                DoSomething2()
            ElseIf (someCondition2) Then

                DoSomething1() ' Noncompliant {{Either merge this branch with the identical one on line 25 or change one of the implementations.}}
                DoSomething2()
            ElseIf (someCondition2) Then

                DoSomething1() ' Noncompliant {{Either merge this branch with the identical one on line 25 or change one of the implementations.}}
                DoSomething2()
            Else
                DoSomething1() ' Noncompliant {{Either merge this branch with the identical one on line 21 or change one of the implementations.}}
                DoSomething1()
            End If

            If (someCondition1) Then
                DoSomething1()
            ElseIf (someCondition2) Then
                DoSomething1() ' Noncompliant
            Else
                DoSomething1() ' Noncompliant
            End If
        End Sub
    End Class
End Namespace
