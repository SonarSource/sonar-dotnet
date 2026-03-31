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

            ' Noncompliant@+2
            Dim namedCust = New Customer With {
                .IsMale = True Or False Or True OrElse False Xor False,
                .IsFemale = False Or True
            }

            If b OrElse b OrElse b OrElse b OrElse _
                b OrElse b OrElse b OrElse b OrElse b Then ' Noncompliant@-1
                b = True
            End If
            If b OrElse b OrElse Not (b OrElse Not b OrElse b) Then ' Noncompliant {{Reduce the number of conditional operators (4) used in the expression (maximum allowed 3).}}
                b = True
            End If
        End Sub
    End Class

    Class Customer
        Property IsMale As Boolean
        Property IsFemale As Boolean
    End Class

    ' https://sonarsource.atlassian.net/browse/USER-1097
    Class VBEqualsTest
        Property A As String
        Property B As String
        Property C As String
        Property D As String

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, VBEqualsTest)
            Return other IsNot Nothing AndAlso A = other.A AndAlso B = other.B AndAlso C = other.C AndAlso D = other.D ' Compliant
        End Function
    End Class

    Class VBEqualsTestTyped
        Implements System.IEquatable(Of VBEqualsTestTyped)

        Property A As String
        Property B As String
        Property C As String
        Property D As String
        Property E As String

        Public Function Equals(other As VBEqualsTestTyped) As Boolean Implements System.IEquatable(Of VBEqualsTestTyped).Equals
            Return other IsNot Nothing AndAlso A = other.A AndAlso B = other.B AndAlso C = other.C AndAlso D = other.D AndAlso E = other.E ' Compliant
        End Function
    End Class

    Class VBEqualsTestTypedRenamedMethod
        Implements System.IEquatable(Of VBEqualsTestTypedRenamedMethod)

        Property A As String
        Property B As String
        Property C As String
        Property D As String
        Property E As String

        Public Function MyEquals(other As VBEqualsTestTypedRenamedMethod) As Boolean Implements System.IEquatable(Of VBEqualsTestTypedRenamedMethod).Equals
            Return other IsNot Nothing AndAlso A = other.A AndAlso B = other.B AndAlso C = other.C AndAlso D = other.D AndAlso E = other.E ' Compliant - method name doesn't matter, it still implements IEquatable.Equals
        End Function
    End Class
End Namespace
