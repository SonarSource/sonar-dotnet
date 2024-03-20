Imports System

Namespace SomeNamespace
  ' Written by a human, not ignored
    Class NotIgnored
        Private Shared Sub Method(ByVal args As String())
            Dim obj As Object = Nothing
            Console.WriteLine(obj.ToString())
        End Sub

        Private Sub Foo()
        End Sub
    End Class
End Namespace

