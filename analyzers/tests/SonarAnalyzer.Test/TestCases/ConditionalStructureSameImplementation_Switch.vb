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
    End Class
End Namespace
