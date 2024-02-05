Module Module1
    Sub Main()
        Dim foo As String() ' Fixed
        Dim foo3(5) As String() ' Fixed
        Dim numbers1(4) As Integer
        Dim numbers2 = New Integer() {1, 2, 4, 8}
        ReDim Preserve numbers1(15)
        Dim matrix(5, 5) As Double
        Dim matrix2 As Double(,) ' Fixed
        Dim matrix3 As Double(,)
        ' Fixed
        Dim matrix4()(),
            aaa As Double
        Dim matrix5 As Double()()
        Dim sales As Double()() = New Double(11)() {}
    End Sub
End Module
