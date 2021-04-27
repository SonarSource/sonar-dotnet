Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
Public Class Program
    Public Sub New()
        Dim choice As String = "Foo"
        Dim value As Integer = 1

        Select Case choice
            Case "Y"c
                Console.WriteLine("Yes")
            Case "M"c
                Console.WriteLine("Maybe")
            Case "N"c
                Console.WriteLine("No")
            Case Else

                    Select Case value ' Noncompliant {{Refactor the code to eliminate this nested 'Select Case'.}}
'                   ^^^^^^^^^^^^^^^^^
                        Case 0
                        Case Else
                    End Select

                    Console.WriteLine("Invalid response")
        End Select

        Dim i As Integer = 1

        Select Case i
            Case 1, 2
                Console.WriteLine("One or Two")
            Case Else
                Console.WriteLine("Other")
        End Select
    End Sub
End Class

End Namespace
