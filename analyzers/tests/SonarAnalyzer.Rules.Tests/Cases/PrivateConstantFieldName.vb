Class Foo
    Private Const Foo = 10  ' Noncompliant
'                 ^^^

    Private ReadOnly foo2 As Integer
    Private ReadOnly s_fooFooFFFoooFF
    Private ReadOnly sfooFooFFFoooFF
    Private ReadOnly sFooFooFFFooooFF
    Private Const _fooFooFFFoooFF = 10, __fooFooFFFoooFF = 42  ' Noncompliant
'                                       ^^^^^^^^^^^^^^^^

    Private Const _fooFooFFFFoooFF As Integer = 1 ' Noncompliant
    Private Const _fooFooFFFoooF As Integer = 2 ' Noncompliant
    Private Shared ReadOnly _fooFooFFFoooFFF As Integer

End Class
