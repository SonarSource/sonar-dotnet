Namespace Tests.TestCases
    Public Class Foo
        Public Shared Property ValProp As Integer

        Sub New(ByVal left As Integer, ByVal right As Integer)

        End Sub

        Sub New()
        End Sub

        Public Sub Invocations()
            Dim divisor = 15
            Dim dividend = 5

            Divide(divisor, dividend)
            Divide(dividend, dividend)
            Divide(divisor, divisor)
            Foo.GlobalDivide(divisor, dividend)

            Divide(dividend, divisor) ' Noncompliant [1] {{Parameters to 'Divide' have the same names but not the same order as the method arguments.}}
'           ^^^^^^
            Foo.GlobalDivide(dividend, divisor) ' Noncompliant [2]
            '   ^^^^^^^^^^^^

            Divide(dividend:=dividend, divisor:= divisor)

            Divide(ValProp, Foo.ValProp)

            FooBar(1)
            FooBar(1, "a", "b")
            FooBar(value:= 1)
		End Sub

        Public Sub ObjectCreations()
            Dim left = 1
            Dim right = 2

            Dim x = New Foo(left, right)
            x = New Foo(left, left)
            x = New Foo(right, right)
            x = New Foo(right:= right, left:= left)
            x = New Foo

            x = New Foo(right, left) ' Noncompliant
            '       ^^^
            x = New FooFoo(right, left) ' Noncompliant
            x = New Foo.FooFoo(right, left) ' Noncompliant
            '           ^^^^^^

            x = New Foo(ValProp, Foo.ValProp)
        End Sub

        Sub Bar(ByVal name As String, Optional ByVal age As Short = 0)
        End Sub

        Sub FooBar(ByVal value As Integer, ByVal ParamArray args() As String)
        End Sub

        Public Function Divide(ByVal divisor As Integer, ByVal dividend As Integer) As Double ' Secondary [1]
            Return divisor / dividend
        End Function

        Public Shared Function GlobalDivide(ByVal divisor As Integer, ByVal dividend As Integer) As Double ' Secondary [2]
            Return divisor / dividend
        End Function

        Public Class FooFoo
            Inherits Foo
            Sub New(ByVal left As Integer, ByVal right As Integer)
                MyBase.New(right, left) ' Noncompliant
            End Sub
        End Class
    End Class
End Namespace
