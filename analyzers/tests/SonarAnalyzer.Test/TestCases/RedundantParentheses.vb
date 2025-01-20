Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class Foo
        Private Dim a As Object

        Public Function Test() As Object
            Return (1)
            Return (a)
            Return False

            Return (((a)))
'                  ^^ Noncompliant [0] {{Remove these redundant parentheses.}}
'                       ^^ Secondary@-1 [0] {{Remove the redundant closing parentheses.}}

            If (a IsNot Nothing) Then
            End If

             If ((a IsNot Nothing)) Then
'               ^ Noncompliant [1]
'                                 ^ Secondary@-1 [1]
            End If

            Dim x = 1
            Dim y = 12
            Dim foo = ((x + 2) / (((y))))
'                                ^^ Noncompliant [2]
'                                     ^^ Secondary@-1 [2]
		End Function
    End Class
End Namespace
