Module Module1
    Sub Main()
        Dim foo As String() ' Fixed
        Dim foo2 As String()() ' Fixed
        Dim foo3(5) As String() ' Fixed
        Dim foo4() As New String()
        Dim numbers1(4) As Integer
        Dim numbers2 = New Integer() {1, 2, 4, 8}
        ReDim Preserve numbers(15)
        Dim matrix(5, 5) As Double
        Dim matrix2 As Double(,) ' Fixed
        Dim matrix3 As Double(,)
        Dim matrix4()(), ' Fixed
            aaa As Double
        Dim matrix5 As Double()()
        Dim sales As Double()() = New Double(11)() {}
        Dim sales2 As Double(5)(4) ' doesn't compile
    End Sub
End Module