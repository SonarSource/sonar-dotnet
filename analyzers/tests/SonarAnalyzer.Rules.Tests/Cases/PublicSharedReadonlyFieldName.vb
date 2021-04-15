Class Foo
    Public Shared ReadOnly Foo As Integer  ' Compliant

    Public Shared ReadOnly foo2 As Integer ' Noncompliant
'                          ^^^^
    Public Shared ReadOnly FooFooFFFoooFF As Integer
    Friend Shared ReadOnly FooFooFFFFoooFF As Integer ' Noncompliant
    Protected Shared ReadOnly FooFooFFFFoooFFF As Integer ' Noncompliant

End Class