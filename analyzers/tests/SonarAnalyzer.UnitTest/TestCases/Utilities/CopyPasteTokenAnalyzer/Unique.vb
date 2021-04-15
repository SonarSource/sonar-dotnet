Public Class Sample

    Public Sub Aaa()
        Dim x As Integer = 42
        Dim InterpolatedWithWhitespaceToken As String = $"This literal should be $str but the whitespace between interpolation will not: {x} {x}"
    End Sub

    Public Sub Bbb()
        Dim a As String = "value"
        Dim b As Integer = 42
        Dim c As Integer = &H2A
        Dim d As Integer = &B101010
        Dim e As Single = 42.42F
        Dim f As Double = 42.42
        Dim g As Decimal = 42.42D
        Dim h As Char = "x"c
    End Sub

End Class
