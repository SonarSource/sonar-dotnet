
Public Class Sample

    Public Function ConditionConditionalAccessAndTernary(Condition As Boolean, A As String, B As String) As String
        If String.IsNullOrEmpty(A?.ToString) Then
            Return If(Condition, A, B)
        Else
            Return "Lorem Ipsum"
        End If
    End Function

End Class
