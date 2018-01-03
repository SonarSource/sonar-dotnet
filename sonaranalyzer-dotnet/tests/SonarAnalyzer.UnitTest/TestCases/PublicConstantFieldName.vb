Class Foo
    Public Shared ReadOnly Foo As Integer  ' Compliant

    Public Const foo2 = 42 ' Noncompliant
'                ^^^^
    Public Shared FooFooFFFoooFF As Integer
    Friend Const FooFooFFFFoooFF As Integer ' Noncompliant
    Protected Const FooFooFFFFoooFFF As Integer ' Noncompliant
    Protected ReadOnly FooFooFFFFoooFFFF As Integer

End Class