Imports System, System.IO

Friend Module Module1

    Class MyOwnClass
        Property MyAutoProperty As String

        Property MyProperty As String
            Get
                Return ""
            End Get
            Set(ByVal value As String)

            End Set
        End Property
    End Class

    Sub Main()
        Console.WriteLine("Hello, world!")
    End Sub

    REM This is a comment
    ' This also is a comment

End Module
