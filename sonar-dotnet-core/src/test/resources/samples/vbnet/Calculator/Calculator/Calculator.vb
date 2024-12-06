Public Class Calculator
    Public Function Add(a As Integer, b As Integer, predicate As Predicate(Of Integer)) As Integer
        Dim sum = a + b
        Return If(predicate(sum), sum, 0)
    End Function
End Class
