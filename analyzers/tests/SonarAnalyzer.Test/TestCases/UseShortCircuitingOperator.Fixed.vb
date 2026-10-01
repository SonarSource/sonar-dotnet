Public Class ClassWithLogicalStatements

    Public Function [And](first As Boolean, second As Boolean) As Boolean

        If first AndAlso second Then ' Fixed
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

        If first OrElse second Then ' Fixed
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
        result = a AndAlso b OrElse c   ' Fixed
        result = a OrElse b AndAlso c   ' Fixed
        result = a AndAlso b Xor c      ' Fixed
        result = a Xor b AndAlso c      ' Fixed
        result = a Xor b OrElse c       ' Fixed
        result = a OrElse b Xor c       ' Fixed
        result = a AndAlso b AndAlso c  ' Fixed
        result = a AndAlso b AndAlso c  ' Fixed
        result = a OrElse b OrElse c    ' Fixed
        result = a OrElse b OrElse c    ' Fixed
        result = a AndAlso b OrElse c   ' Fixed
        result = a OrElse b AndAlso c   ' Fixed
        result = a AndAlso (b OrElse c) ' Fixed
        result = (a AndAlso b) Xor c    ' Fixed
        result = Not (a OrElse b)       ' Fixed
        result = If(a OrElse b, a, c)   ' Fixed
        result = a AndAlso b OrElse c       ' Fixed
        result = a OrElse b AndAlso c       ' Fixed
        result = a AndAlso (b OrElse c)     ' Fixed
    End Sub

End Class
