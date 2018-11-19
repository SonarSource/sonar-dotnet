Class Foo
    Public Shared ReadOnly Foo As Integer  ' Compliant

    Public Const foo2 = 42 ' Noncompliant
'                ^^^^
    Public Shared FooFooFFFoooFF As Integer
    Friend Const FooFooFFFFoooFF As Integer = 1 ' Noncompliant
    Protected Const FooFooFFFFoooFFF As Integer = 1 ' Noncompliant
    Protected ReadOnly FooFooFFFFoooFFFF As Integer

End Class
