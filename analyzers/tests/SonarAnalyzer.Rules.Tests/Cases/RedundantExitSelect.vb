Imports System

Module Module1
    Sub Main()
        Dim x = 0
        Select Case x
            Case 0
                Console.WriteLine("0")
                Exit Select                ' Noncompliant {{Remove this redundant use of 'Exit Select'.}}
'               ^^^^^^^^^^^
            Case 1
                Console.WriteLine("1")
                If True Then
                    Exit Select ' Compliant
                End If
                Exit Sub
            Case Else
                Console.WriteLine("Not 0, 1")
                Exit Select                ' Noncompliant
        End Select
    End Sub
End Module
