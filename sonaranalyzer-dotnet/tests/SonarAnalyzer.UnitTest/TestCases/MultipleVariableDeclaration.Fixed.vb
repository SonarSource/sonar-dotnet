Namespace Tests.Diagnostics

    Public Class MultipleVariableDeclaration
        Public Const AAA As Integer = 5
        Public Const BBB = 42
        Public Const DDD As String = "foo"
        Public Sub New(n As Integer)
            Dim Const AAA As Integer
            Dim Const B As Integer = 5
            Dim Const BBB = 42
            Dim Const CCC As String = "foo"
            Dim Const DDD As String = "foo"
        End Sub
    End Class
End Namespace