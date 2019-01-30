Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic

Namespace Tests.TestCases
    Class Foo
        Public Sub Test()
            Dim number As Integer = 8

            Select Case number
                Case 1 To 5 ' Noncompliant {{Reduce this 'Case' block number of statements from 9 to at most 8, for example by extracting code into a procedure.}}
'               ^^^^^^^^^^^
                    Test()
                    Test()
                    Test()
                    Test()
                    Test()
                    Test()
                    Test()
                    Test()
                    Test()
                Case 6 ' Noncompliant {{Reduce this 'Case' block number of statements from 12 to at most 8, for example by extracting code into a procedure.}}
                    If False
                    ElseIf True
                    Else
                        For index = 1 To 10
                        Next
                        While True
                        End While
                        Do
                        Loop While (number < 1)
                        Do
                        Loop Until (number = 1)
                        Select Case number
                            Case 1
                            Case 2
                            Case Else
                        End Select
                        If True Then Test()
                    End If
                Case Else
                    Test()
            End Select
		End Sub
    End Class
End Namespace
