Namespace Tests.Diagnostics
    Interface IA
    End Interface
    Class A
        Implements IA
    End Class

    Class Foo
        Public Sub Foo2(a As Decimal, b As Decimal)
        End Sub
        Public Sub Foo2(a As Double, b As Double)
        End Sub
        Public Sub Foo2(a As Boolean, b As Boolean)
        End Sub
        Public Sub Foo2(a As String, b As String)
        End Sub
        Public Sub Foo2(a As Integer, b As Integer)
        End Sub

        Public Sub DifferentUsages(a As IA, b As A)
        End Sub
        Public Sub Foo2(a As A, b As A)
        End Sub

        Public Sub Foo5(a As Integer, b As Integer, c As Integer, d As Integer, e As Integer)
        End Sub
        Public Sub Bar(a As String, b As Integer)
        End Sub

        Public Sub FooInt(x As Integer, y As Integer)
        End Sub

        Public Sub Parameterless()
        End Sub

        Private x As String

        Public Sub Test()
            Dim x As Integer, y As Integer

            Foo5(x, x, x, x, x)
'                   ^ Noncompliant    {{Verify that this is the intended value; it is the same as the 1st argument.}}
'                ^ Secondary@-1
'                      ^ Noncompliant@-2    {{Verify that this is the intended value; it is the same as the 1st argument.}}
'                ^ Secondary@-3
'                         ^ Noncompliant@-4    {{Verify that this is the intended value; it is the same as the 1st argument.}}
'                ^ Secondary@-5
'                            ^ Noncompliant@-6    {{Verify that this is the intended value; it is the same as the 1st argument.}}
'                ^ Secondary@-7

            Foo5(x, 1, 1, 1, x) ' Noncompliant {{Verify that this is the intended value; it is the same as the 1st argument.}}
' Secondary@-1
            Foo5(1, x, 1, 1, x) ' Noncompliant {{Verify that this is the intended value; it is the same as the 2nd argument.}}
' Secondary@-1
            Foo5(1, 1, x, 1, x) ' Noncompliant {{Verify that this is the intended value; it is the same as the 3rd argument.}}
' Secondary@-1
            Foo5(1, 1, 1, x, x) ' Noncompliant {{Verify that this is the intended value; it is the same as the 4th argument.}}
' Secondary@-1
            Foo2(True, True)
            Dim b As Boolean
            Foo2(b, b) ' Noncompliant
            ' Secondary@-1
            Bar(Me.x, x)

            FooInt(x, y)
            Foo2(x.ToString(), y.ToString())
            Foo2("x", "x")
            Foo2("x", "x")
            Foo2("x", "x")
            Foo2(x.ToString(), x.ToString()) ' Noncompliant
' Secondary@-1
            Dim foo1 As Foo, foo2__1 As Foo
            Foo2(foo1.x, foo1.x) ' Noncompliant
' Secondary@-1
            ' My comment
            Foo2(foo1.x, foo1.x) ' Noncompliant
' Secondary@-1;
            Foo2(foo1.x, foo2__1.x)

            Foo2(-1, -1)
            Foo2(1.0, 1.0)
            Foo2(1D, 1D)

            Dim a As New A()
            DifferentUsages(a, a)
            Foo2(a, a) ' Noncompliant
' Secondary@-1;

            Me.Parameterless
        End Sub
    End Class
End Namespace
