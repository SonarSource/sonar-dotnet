Imports System

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
            Foo.GlobalDivide(dividend, divisor) ' Noncompliant

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
            x = New FooFoo(right, left) ' Noncompliant
            x = New Foo.FooFoo(right, left) ' Noncompliant

            x = New Foo(ValProp, Foo.ValProp)
        End Sub

        Sub Bar(ByVal name As String, Optional ByVal age As Short = 0)
        End Sub

        Sub FooBar(ByVal value As Integer, ByVal ParamArray args() As String)
        End Sub

        Public Function Divide(ByVal divisor As Integer, ByVal dividend As Integer) As Double ' Should be secondary-location for [1], but does not work
            Return divisor / dividend
        End Function

        Public Shared Function GlobalDivide(ByVal divisor As Integer, ByVal dividend As Integer) As Double
            Return divisor / dividend
        End Function

        Public Class FooFoo
            Inherits Foo
            Sub New(ByVal left As Integer, ByVal right As Integer)
                MyBase.New(right, left) ' Noncompliant
            End Sub
        End Class
    End Class

    ' See https://github.com/SonarSource/sonar-dotnet/issues/3879
    Public Class NotOnlyNullableParam

        Public Class A
            Public Property Something as C
        End Class

        Public Class C
            Public Property b as Integer
        End Class

        Public Class B
            Public Property Value as Integer
        End Class

        Sub NotNullableParamVoid(ByVal a as Integer, ByVal b as Integer)
            ' Do nothing
        End Sub

        Sub NullableParamValueVoid(ByVal a as Integer, ByVal b as Nullable(Of Integer))

            if b.HasValue Then
                NotNullableParamVoid(b.Value, a) ' Noncompliant
                NotNullableParamVoid(a, b.Value) ' Compliant
            End If
        End Sub


        Sub NullableParamCastVoid(ByVal a as Integer, ByVal b as Integer?)
            if b.HasValue Then
                NotNullableParamVoid(CInt(b), a) ' Noncompliant
                NotNullableParamVoid(a, CInt(b)) ' Compliant
            End If
        End Sub

        Sub InnerPropertyParamVoid(ByVal a as Integer, ByRef c as A)
            NotNullableParamVoid(c.Something.b, a) ' Noncompliant
            NotNullableParamVoid(a, c.Something.b) ' Compliant
        End Sub

        Sub InnerPropertyParamVoid(ByVal c as Integer, ByRef b as B)
            NotNullableParamVoid(b.Value, c) ' Compliant
            NotNullableParamVoid(c, b.Value) ' Compliant
        End Sub

       Sub ObjectParamCastVoid(ByVal a as Integer, ByVal b as Object)
            NotNullableParamVoid(CInt(b), a) ' Noncompliant
            NotNullableParamVoid(a, CInt(b)) ' Compliant
       End Sub

        Sub ObjectParamNullableCastVoid(ByVal a as Integer, ByVal b as Object)
            NotNullableParamVoid(DirectCast(b, Integer?).Value, a) ' Noncompliant
            NotNullableParamVoid(a, DirectCast(b, Integer?).Value) ' Compliant
        End Sub

        Sub DifferentCaseParamsVoid(ByVal a as Integer, ByVal B as Integer)
            NotNullableParamVoid(B, a) ' Noncompliant
            NotNullableParamVoid(a, B) ' Compliant
        End Sub
    End Class
End Namespace
