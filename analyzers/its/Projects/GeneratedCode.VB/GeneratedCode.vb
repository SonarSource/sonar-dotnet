Imports System
Imports System.CodeDom.Compiler

Class Class_12
    Private Shared Sub Method(ByVal args As String())
        Dim obj As Object = Nothing
        Console.WriteLine(obj.ToString())
    End Sub

    <GeneratedCode("foo", "bar")>
    Private Sub Foo()
    End Sub
End Class
