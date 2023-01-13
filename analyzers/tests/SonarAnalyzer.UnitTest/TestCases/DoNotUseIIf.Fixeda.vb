Imports Microsoft.VisualBasic

Namespace Tests.TestCases
    Class Foo
        Public Sub IIf_Should_Not_Be_Used()
            Dim obj As Object = If(Date.Now.Year = 1999, "Lets party!", "Lets party like it is 1999!") ' Fixed
        End Sub
    End Class
    Class Bar
        Public Sub Method_With_Same_Name()
            Dim obj As Object = IIf(Date.Now.Year = 1999, "Lets party!", "Lets party like it is 1999!")
        End Sub
        Public Function IIf(condition As Boolean, arg1 As Object, arg2 As Object) As Object
            If (condition) Then
                Return arg1
            End If
            Return arg2
        End Function
    End Class
End Namespace
