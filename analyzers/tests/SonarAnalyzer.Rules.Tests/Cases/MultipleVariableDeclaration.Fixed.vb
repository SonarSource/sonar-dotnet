Namespace Tests.Diagnostics

    Public Class MultipleVariableDeclaration
        Public Const AAA As Integer = 5
        Public Const BBB = 42
        Public Const DDD As String = "foo"
        Public Sub New(n As Integer)
            Dim AAA = 1
            Dim B As Integer = 5
            Dim BBB = 42
            Dim CCC As String = "foo"
            Dim DDD As String = "foo"
        End Sub
    End Class
End Namespace
