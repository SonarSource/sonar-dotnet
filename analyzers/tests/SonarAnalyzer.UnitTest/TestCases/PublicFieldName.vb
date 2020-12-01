Class Foo
    Public Shared ReadOnly Foo As Integer  ' Compliant

    Public Shared foo2 As Integer ' Noncompliant
'                 ^^^^
    Public Shared FooFooFFFoooFF As Integer
    Friend ReadOnly FooFooFFFFoooFF As Integer ' Noncompliant
    Protected FooFooFFFFoooFFF As Integer ' Noncompliant
    Protected Const FooFooFFFFoooFFFF As Integer = 2

End Class
