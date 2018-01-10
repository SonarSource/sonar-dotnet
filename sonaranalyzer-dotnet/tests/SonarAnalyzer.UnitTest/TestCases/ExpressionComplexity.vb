Imports System

Namespace NS
    Class CL
        Sub Method(b As Boolean)
            Dim x = True Or False Or True
            x = True Or False Or True OrElse False Xor False ' Noncompliant {{Reduce the number of conditional operators (4) used in the expression (maximum allowed 3).}}

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

            Dim menuOptions = New List(Of Boolean) From {
                {1, True Or False Or True OrElse False Xor False}, ' Noncompliant
                {2, rue Or False Or True OrElse False}}
        End Sub
    End Class
End Namespace