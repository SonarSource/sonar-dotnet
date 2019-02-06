Namespace Tests.TestCases
    Class Foo
        Sub New(ByVal left As Integer, ByVal right As Integer)

        End Sub

        Public Sub Invocations()
            Dim divisor = 15
            Dim dividend = 5

            Divide(divisor, dividend)
            Divide(dividend, dividend)
            Divide(divisor, divisor)

            Divide(dividend, divisor) ' Noncompliant {{Parameters to 'Divide' have the same names but not the same order as the method arguments.}}

            Divide(dividend:=dividend, divisor:= divisor)
		End Sub

        Public Sub ObjectCreations()
            Dim left = 1
            Dim right = 2

            Dim x = New Foo(left, right)
            x = New Foo(left, left)
            x = New Foo(right, right)
            x = New Foo(right:= right, left:= left)

            x = New Foo(right, left) ' Noncompliant
        End Sub

        Sub Display(ByVal name As String, Optional ByVal age As Short = 0, Optional ByVal birth As Date = #1/1/2000#)
        End Sub

        Public Function Divide(ByVal divisor As Integer, ByVal dividend As Integer) As Double
            Return divisor / dividend
        End Function
    End Class
End Namespace
