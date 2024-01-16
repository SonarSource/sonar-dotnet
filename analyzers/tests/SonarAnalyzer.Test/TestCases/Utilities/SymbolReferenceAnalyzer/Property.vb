Public Class Sample

    Public Property Name As String

    Public Sub Method()
        Dim n As String = Name
        Name = "John"
        Dim x = name ' Different case, same property
    End Sub

End Class
