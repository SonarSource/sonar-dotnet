Class Foo
    Private ReadOnly Foo As Integer  ' Noncompliant
'                    ^^^

    Private ReadOnly foo2 As Integer
    Private ReadOnly s_fooFooFFFoooFF
    Private ReadOnly sfooFooFFFoooFF
    Private ReadOnly sFooFooFFFooooFF
    Private ReadOnly _fooFooFFFoooFF, __fooFooFFFoooFF As Integer ' Noncompliant
'                                     ^^^^^^^^^^^^^^^^

    Private Shared _fooFooFFFFoooFF As Integer ' Noncompliant
    Private Shared _fooFooFFFoooF As Integer ' Noncompliant
    Private Const _fooFooFFFoooFFF As Integer = 1
    Private Shared ReadOnly _fooFooFFFFoooFFF As Integer

End Class
