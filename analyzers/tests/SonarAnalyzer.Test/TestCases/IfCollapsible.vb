Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class Foo
        Public Sub Test(ByVal cond1 As Boolean, ByVal cond2 As Boolean, ByVal cond3 As Boolean)
            If cond1 Then
            End If

            If cond1 Then ' Secondary [0] {{Merge this if statement with its nested one.}}
                If cond2 Then ' Noncompliant [0] {{Merge this if statement with the enclosing one.}}
                End If
            End If

            If cond1 Then
                If cond2 Then
                Else
                End If
            End If

            If cond1 Then
                If cond2 Then ' Compliant - parent as a Else clause
                End If
            Else
            End If

            If cond1 Then
                Dim x = 1
                If cond2 Then ' Compliant - not direct child of the previous if
                End If
            End If

            If cond1 Then
'           ^^ Secondary [1]
                If cond2 OrElse cond3 Then
'               ^^ Noncompliant [1]
                End If
            End If

            If cond1 Then
            ElseIf cond2 Then
                If cond3 Then
                End If
            End If
		End Sub
    End Class
End Namespace
