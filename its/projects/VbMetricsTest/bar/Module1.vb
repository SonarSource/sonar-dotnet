Imports System, System.IO

REM This is a comment!

Public Module bar
    ''' <summary>
    ''' Documented public API
    ''' </summary>
    Public Class MyClazz
        Public ReadOnly Property MyProperty As Integer
            Get
                Return 42
            End Get
        End Property
    End Class

    Sub Main(ByVal condition As Boolean)
        Dim myClazz As New MyClazz
        If condition Then
            Console.WriteLine("Hello, world! " & myClazz.MyProperty)
            Console.ReadLine()
        End If
    End Sub
End Module
