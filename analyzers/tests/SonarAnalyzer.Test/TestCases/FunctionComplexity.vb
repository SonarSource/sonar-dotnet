Namespace Tests.Diagnostics
    Public Class FunctionComplexityVisualBasic
        Public Sub New() ' Noncompliant {{The Cyclomatic Complexity of this constructor is 4 which is greater than 3 authorized.}}
            If False Then

            End If
            If False Then

            End If
            If False Then

            End If
        End Sub

        Public Sub S1() ' Compliant
            If False Then

            End If
            If False Then

            End If
        End Sub

        Public Sub S2()  ' Noncompliant
            If False Then

            End If
            If False Then

            End If
            If False Then

            End If
        End Sub

        Public Function F1() As Integer  ' Compliant
            If False Then

            End If
            If False Then

            End If
            Return 0
        End Function

        Public Function F2() As Integer  ' Noncompliant
            If False Then

            End If
            If False Then

            End If
            If False Then

            End If
            Return 0
        End Function

        Public Shared Operator ^(ByVal left As FunctionComplexityVisualBasic, ByVal right As FunctionComplexityVisualBasic) As Integer  ' Compliant
            If False Then

            End If
            If False Then

            End If
            Return 0
        End Operator

        Public Shared Operator &(ByVal left As FunctionComplexityVisualBasic, ByVal right As FunctionComplexityVisualBasic) As Integer  ' Noncompliant
            If False Then

            End If
            If False Then

            End If
            If False Then

            End If
            Return 0
        End Operator

        Public Property P1 As Integer
            Get ' Compliant
                If False Then

                End If
                If False Then

                End If
                Return 0
            End Get
            Set(value As Integer) ' Compliant
                If False Then

                End If
                If False Then

                End If
            End Set
        End Property

        Public Property P2 As Integer
            Get ' Noncompliant
                If False Then

                End If
                If False Then

                End If
                If False Then

                End If
                If False Then

                End If
                Return 0
            End Get
            Set(value As Integer) ' Noncompliant
                If False Then

                End If
                If False Then

                End If
                If False Then

                End If
                If False Then

                End If
            End Set
        End Property
    End Class
End Namespace
