Imports System

Module Module1
    Sub LineContinuation()
        ' Noncompliant@+1 {{Reformat the code to remove this use of the line continuation character.}}
        Console.WriteLine("Hello" _
                          & "world")
    End Sub
End Module
