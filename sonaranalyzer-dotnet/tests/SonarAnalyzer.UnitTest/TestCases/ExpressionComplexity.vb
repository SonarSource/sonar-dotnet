Imports System
Imports System.Collections.Generic

Namespace NS
    Class CL
        Sub Method(b As Boolean)
            Dim xx = True Or False Or True
            xx = True Or False Or True OrElse False Xor False ' Noncompliant {{Reduce the number of conditional operators (4) used in the expression (maximum allowed 3).}}

            Method(
                True Or False Or True OrElse False Xor False) ' Noncompliant

            Dim funcLambda = Function(x)
                                 Return True Or False Or True OrElse False Xor False ' Noncompliant
                             End Function

            funcLambda = Function(x)
                             Return True Or False
                         End Function

            Dim subLambda = Sub(x)
                                Console.Write(
                                        True Or False Or True OrElse False Xor False) ' Noncompliant
                            End Sub

            subLambda = Sub(x)
                            Console.Write(
                                        True Or False Or True OrElse False)
                        End Sub

            funcLambda = Function(x) True Or False Or True OrElse False Xor False ' Noncompliant
'                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            subLambda = Sub(x) Console.Write(
                                        True Or False Or True OrElse False Xor False) ' Noncompliant

            Dim namedCust = New Customer With {
                .IsMale = True Or False Or True OrElse False Xor False, ' Noncompliant
                .IsFemale = False Or True
            }
        End Sub
    End Class

    Class Customer
        Property IsMale As Boolean
        Property IsFemale As Boolean
    End Class
End Namespace
