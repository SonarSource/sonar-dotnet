Imports System

Namespace Tests.Diagnostics
    Public Class Program
        Public Sub IfElseCases(b As Integer, c As Integer)
            If b = 0 Then 'Noncompliant
'           ^^
                DoSomething()
            ElseIf b = 1 Then
                DoSomething()
            Else
                DoSomething()
            End If

            If b = 0 Then 'Noncompliant
                If c = 1 Then 'Noncompliant
                    DoSomething()
                Else
                    DoSomething()
                End If
            Else
                If c = 1 Then 'Noncompliant
                    DoSomething()
                Else
                    DoSomething()
                End If
            End If

            If b = 0 Then
                DoSomething()
            Else
                DoSomethingElse()
            End If

            If b = 0 Then 'Compliant, no else
                DoSomething()
            ElseIf b = 1 Then
                DoSomething()
            End If
        End Sub

        Public Sub SwitchCases(i As Integer)
            Select Case i ' Noncompliant {{Remove this conditional structure or edit its code blocks so that they're not all the same.}}
'           ^^^^^^
                Case 1
                    DoSomething()
                Case 2
                    DoSomething()
                Case 3
                    DoSomething()
                Case Else
                    DoSomething()
            End Select

            Select Case i ' Noncompliant
                Case 1
                    DoSomething()
                    Exit Select
                Case 2
                    DoSomething()
                    Exit Select
                Case 3
                    DoSomething()
                    Exit Select
                Case Else
                    DoSomething()
                    Exit Select
            End Select

            Select Case i
                Case 1
                    DoSomething()
                Case Else
                    DoSomethingElse()
            End Select

            Select Case i
                Case 1
                    DoSomething()
                Case 2
                    DoSomething()
                Case 3
                    DoSomething()
            End Select
        End Sub

        Public Sub TernaryCases(ByVal c As Boolean)
            Dim a As Integer = 1
            Dim b As Integer = If(a > 12, 4, 4) 'Noncompliant {{This conditional operation returns the same value whether the condition is "true" or "false".}}
'                              ^^
            Dim x = If(1 > 18, True, True) 'Noncompliant
            Dim y = If(1 > 18, True, False)
            y = If(1 > 18, (True), True) 'Noncompliant
            TernaryCases(If(1 > 18, (True), True)) 'Noncompliant
        End Sub

        Public Sub SingleLineIfCases(ByVal c As Boolean)
            Dim x As Integer
            If c Then x = 4 Else x = 4 'Noncompliant {{Remove this conditional structure or edit its code blocks so that they're not all the same.}}
'           ^^
            If c Then x = 0 Else x = 1
        End Sub

        Private Sub DoSomething()
        End Sub

        Private Sub DoSomethingElse()
        End Sub
    End Class
End Namespace
