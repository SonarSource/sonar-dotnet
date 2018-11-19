Imports System

Module Module1
    Sub Main()
        Dim i = 1
        Dim ctrl = New Object()

        Do                        ' Noncompliant {{Use a structured loop instead.}}
            If i = 10 Then
                Exit Do
            End If

            Console.WriteLine(i)

            i = i + 1
        Loop
        Do While i <> 10
            Console.WriteLine(i)

            i = i + 1
        Loop
        Do

        Loop Until ctrl Is Nothing
    End Sub
End Module
