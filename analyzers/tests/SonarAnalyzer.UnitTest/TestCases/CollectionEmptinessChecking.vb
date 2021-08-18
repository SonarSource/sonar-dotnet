Imports System
Imports System.System.Collections.Generic
Imports System.Linq

Class OnIEnumerableOfT

    Shared Function SizeDependent(items As IEnumerable(Of String)) As Boolean
        Return items.Count() <> 2 ' Compliant
    End Function

    Shared Function CountGreaterThenZero(items As IEnumerable(Of String)) As Boolean
        Return items.Count() > 0 ' Noncompliant {{Use '.Any()' to test whether this 'IEnumerable<>' is empty or not.}}
        '            ^^^^^
    End Function

    Shared Function ZeroLessThenCount(items As IEnumerable(Of String)) As Boolean
        Return 0 < items.Count() ' Noncompliant
    End Function

    Shared Function CountGreaterThenOrEqualOne(items As IEnumerable(Of String)) As Boolean
        Return items.Count() >= 1 ' Noncompliant
    End Function

    Shared Function OneLessThenOrEqualCount(items As IEnumerable(Of String)) As Boolean
        Return 1 <= items.Count() ' Noncompliant
    End Function

    Shared Function CountEqualsZero(items As IEnumerable(Of String)) As Boolean
        Return items.Count() = 0 ' Noncompliant
    End Function

    Shared Function ZeroEqualsCount(items As IEnumerable(Of String)) As Boolean
        Return 0 = items.Count() ' Noncompliant
    End Function

    Shared Function CountNotEqualZero(items As IEnumerable(Of String)) As Boolean
        Return items.Count() <> 0 ' Noncompliant
    End Function

    Shared Function ZeroNotEqualCount(items As IEnumerable(Of String)) As Boolean
        Return 0 <> items.Count() ' Noncompliant
    End Function

    Shared Function HasContentWithCondition(items As IEnumerable(Of Integer)) As Boolean
        Return items.Count(Function(n) n Mod 2 = 0) > 0 'Noncompliant
    End Function

    Shared Function IsEmptyWithCondition(items As IEnumerable(Of Integer)) As Boolean
        Return items.Count(Function(n) n Mod 2 = 0) = 0 'Noncompliant
    End Function

End Class

Class OnEnumerable
    Shared Function CountGreaterThenZero(items As IEnumerable(Of String)) As Boolean
        Return Enumerable.Count(items) > 0 ' Noncompliant
    End Function
End Class

Class WithAlternativeLiterals

    Const Zero As Integer = 0

    Shared Function EmptyWithConstant(items As IEnumerable(Of String)) As Boolean
        Return items.Count() = Zero ' FP
    End Function

    Shared Function EmptyWithHexadecimal(items As IEnumerable(Of String)) As Boolean
        Return items.Count() = &H0 ' Noncompliant
    End Function

    Shared Function EmptyWithBinary(items As IEnumerable(Of String)) As Boolean
        Return items.Count() = &B0 ' Noncompliant
    End Function

End Class
