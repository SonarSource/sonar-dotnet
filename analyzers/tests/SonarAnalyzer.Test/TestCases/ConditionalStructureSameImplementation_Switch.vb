Namespace Tests.TestCases

    Class ConditionalStructureSameCondition_Switch
        Public Sub Test(i As Integer, prop As Object)

            Select Case (i)

                Case 1
                    doSomething(prop)
                Case 2
                    doSomethingDifferent()
                Case 3
                    doSomething(prop) ' Compliant, single line
                Case 4
                    doSomething2()
                    doSomething2()


                Case 5

                    'some comment here and there
                    doSomething2() ' Noncompliant {{Either merge this case with the identical one on line 15 or change one of the implementations.}}
                    doSomething2()

                Case Else
                    doSomething(prop) ' Compliant, single line

            End Select
        End Sub

        Private Function doSomething(data As Object)
            Return Nothing
        End Function
        Private Function doSomethingDifferent()
            Return Nothing
        End Function
        Private Function doSomething2()
            Return Nothing
        End Function

        Private Function ExceptionOfException(a As Integer)
            Select Case a
                Case 1
                    doSomething2()
                Case 2
                    doSomething2() ' Noncompliant
            End Select
        End Function

        Public Sub Exception(a As Integer)
            Select Case True
                Case a >= 0 AndAlso a < 10
                    doSomething2()
                Case a >= 10 AndAlso a < 20
                    doSomethingDifferent()
                Case a >= 20 AndAlso a < 50
                    doSomething2()
            End Select
        End Sub
    End Class
End Namespace
