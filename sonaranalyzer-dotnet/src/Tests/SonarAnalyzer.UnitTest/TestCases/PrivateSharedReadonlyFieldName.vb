Class Foo
    Private Shared ReadOnly Foo As Integer  ' Noncompliant
'                           ^^^

    Private Shared ReadOnly foo2 As Integer
    Private Shared ReadOnly s_fooFooFFFoooFF
    Private Shared ReadOnly sfooFooFFFoooFF
    Private Shared ReadOnly sFooFooFFFooooFF
    Private Shared ReadOnly _fooFooFFFoooFF, __fooFooFFFoooFF As Integer ' Noncompliant
'                                            ^^^^^^^^^^^^^^^^

    Private Shared ReadOnly _fooFooFFFFoooFF As Integer ' Noncompliant
    Private Shared ReadOnly _fooFooFFFoooF As Integer ' Noncompliant

End Class