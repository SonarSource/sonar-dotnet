Public Class ClassWithLogicalStatements

    Public Function [And](first As Boolean, second As Boolean) As Boolean

        If first And second Then ' Noncompliant {{Correct this 'And' to 'AndAlso'.}}
'                ^^^
            Return True
        End If
        Return False

    End Function

    Public Function [AndAlso](first As Boolean, second As Boolean) As Boolean

        If first AndAlso second Then ' Compliant, using AndAlso.
            Return True
        End If
        Return False

    End Function

    Public Function [Or](first As Boolean, second As Boolean) As Boolean

        If first Or second Then ' Noncompliant {{Correct this 'Or' to 'OrElse'.}}
            Return True
        End If
        Return False

    End Function

    Public Function [OrElse](first As Boolean, second As Boolean) As Boolean

        If first OrElse second Then ' Compliant, using OrElse.
            Return True
        End If
        Return False

    End Function

    Public Function [And](first As Integer, second As Boolean) As Integer
        Return first And second ' Compliant, bitwise operators
    End Function

    Public Function [Or](first As Integer, second As Boolean) As Integer
        Return first Or second ' Compliant, bitwise operators
    End Function

    Public Sub OperatorPrecedence(a As Boolean, b As Boolean, c As Boolean)
        ' And and AndAlso share precedence, as do Or and OrElse.
        Dim result As Boolean
        result = a AndAlso b Or c   ' Noncompliant
        result = a Or b AndAlso c   ' Noncompliant
        result = a And b Xor c      ' Noncompliant
        result = a Xor b And c      ' Noncompliant
        result = a Xor b Or c       ' Noncompliant
        result = a Or b Xor c       ' Noncompliant
        result = a AndAlso b And c  ' Noncompliant
        result = a And b AndAlso c  ' Noncompliant
        result = a OrElse b Or c    ' Noncompliant
        result = a Or b OrElse c    ' Noncompliant
        result = a And b OrElse c   ' Noncompliant
        result = a OrElse b And c   ' Noncompliant
        result = a AndAlso (b Or c) ' Noncompliant
        result = (a And b) Xor c    ' Noncompliant
        result = Not (a Or b)       ' Noncompliant
        result = If(a Or b, a, c)   ' Noncompliant
        result = a And b Or c       ' Noncompliant [andBeforeOr, orAfterAnd]
        result = a Or b And c       ' Noncompliant [orBeforeAnd, andAfterOr]
        result = a And (b Or c)     ' Noncompliant [outerAnd, innerOr]
    End Sub

End Class
