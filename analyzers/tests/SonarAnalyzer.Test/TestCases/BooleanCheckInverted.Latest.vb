Namespace CSharp15

    Public Class NumericTypes

        Public Sub TestIeee754(a As System.Numerics.Decimal32, b As System.Numerics.Decimal32, c As System.Numerics.BFloat16, d As System.Numerics.BFloat16)
            Dim result As Boolean = Not (a < b) 'Compliant, Decimal32 can be NaN https://sonarsource.atlassian.net/browse/NET-4890
            result = Not (c > d) 'Compliant, BFloat16 can be NaN https://sonarsource.atlassian.net/browse/NET-4890
            result = Not (a = b) 'Noncompliant
            result = Not (c <> d) 'Noncompliant
        End Sub

    End Class

End Namespace
