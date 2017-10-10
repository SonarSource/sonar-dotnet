Namespace Tests.Diagnostics

    Public Class MultipleVariableDeclaration
        Public Const AAA As Integer = 5, 'some comment
            BBB = 42  ' Noncompliant {{Declare 'BBB' in a separate statement.}}
'           ^^^
        Public Const DDD As String = "foo"
        Public Sub New(n As Integer)
            Dim Const AAA, B As Integer = 5, ' Noncompliant
                BBB = 42, ' Noncompliant
                CCC As String = "foo"  ' Noncompliant
            Dim Const DDD As String = "foo"
        End Sub
    End Class
End Namespace