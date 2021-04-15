Namespace Tests.Diagnostics

    Public Class MultipleVariableDeclaration
        Public Const AAA As Integer = 5, 'some comment
            BBB = 42  ' Noncompliant {{Declare 'BBB' in a separate statement.}}
'           ^^^
        Public Const DDD As String = "foo"
        Public Sub New(n As Integer)
            Dim AAA = 1, B As Integer = 5, ' Noncompliant
                BBB = 42, ' Noncompliant
                CCC As String = "foo"  ' Noncompliant
            Dim DDD As String = "foo"
        End Sub
    End Class
End Namespace
