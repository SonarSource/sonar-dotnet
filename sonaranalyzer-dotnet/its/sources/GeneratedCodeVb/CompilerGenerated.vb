Imports System
Imports System.Runtime.CompilerServices

Public Class CompilerGenerated
    Sub Main(arg As String)
        Dim obj As Object = Nothing
        Console.WriteLine(obj.ToString())
    End Sub

    <CompilerGenerated>
    Sub Foo()
    End Sub

End Class
