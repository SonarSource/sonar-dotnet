Option Strict On

Imports System

Module Main
    Sub Main(args As String())
        Dim Foo = 0 ' Noncompliant S117, S1481
        Console.WriteLine("Hello World!")
    End Sub
End Module
