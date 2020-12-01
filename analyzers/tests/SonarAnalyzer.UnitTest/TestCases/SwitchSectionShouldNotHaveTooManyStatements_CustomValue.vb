Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class Foo
        Public Sub Test()
            Dim number As Integer = 8

            Select Case number
                Case 1 To 5 ' Noncompliant {{Reduce this 'Case' block number of statements from 3 to at most 1, for example by extracting code into a procedure.}}
'               ^^^^^^^^^^^
                    Test()
                    Test()
                    Test()
                Case 6
                    Test()
                Case Else ' Noncompliant
                    Test()
                    Test()
                    Test()
            End Select

            Select Case number
                Case 1 To 5
                Case 6
                Case Else
            End Select

		End Sub
    End Class
End Namespace
