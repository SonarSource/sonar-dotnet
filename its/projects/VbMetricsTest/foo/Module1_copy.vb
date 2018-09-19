Imports System, System.IO

REM This is a comment!

Public Module foo2
    Public Class MyClazz
        Public ReadOnly Property MyProperty As Integer
            Get
                Return 42
            End Get
        End Property
    End Class

    Sub Main()
        Dim myClazz As New MyClazz
        Console.WriteLine("Hello, world! " & myClazz.MyProperty)
        Console.ReadLine()
    End Sub
End Module
