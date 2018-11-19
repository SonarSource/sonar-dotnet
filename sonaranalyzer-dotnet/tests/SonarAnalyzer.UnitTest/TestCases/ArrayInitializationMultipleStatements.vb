Module Module1
    Sub Main()
        Const i As Integer = 0
        Dim f2(i) As String ' Noncompliant {{Refactor this code to use the '... = {}' syntax.}}
'       ^^^^^^^^^^^^^^^^^^^
        f2(0) = "foo"
        Dim f As String
        Dim foo(1) As String      ' Noncompliant
        foo(0) = "foo"
        foo(1) = "bar"
        foo = {"foo", "bar"}  ' Compliant

        Dim foo2 As String(), foo4 As String() = {},
            foo3(1) As String   'compliant, not a single VarDeclarator in the declaration
        foo3(0) = "foo"
        foo2(0) = "foo"
        foo2(1) = "bar"

        Dim f3(3) As String
        f3(0) = "foo"
        f3(2) = "foo"

        Dim f4(1) As String ' Noncompliant
        f4(-1) = "foo"
        f4(0) = "foo"
        f4(1) = "foo"
    End Sub
End Module
