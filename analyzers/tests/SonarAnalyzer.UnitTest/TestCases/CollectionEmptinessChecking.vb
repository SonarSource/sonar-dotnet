Imports System.Collections.Generic
Imports System.Linq

Namespace Tests.Diagnostics

    Public Class CollectionEmptinessChecking

        Private Function HasContent1(l As IEnumerable(Of String)) As Boolean
            Return l.Count() > 0 ' Noncompliant {{Use '.Any()' to test whether this IEnumerable is empty or not.}}
            '        ^^^^^
        End Function

        Private Function HasContent1b(l As IEnumerable(Of String)) As Boolean
            Return 0 < l.Count() ' Noncompliant
        End Function

        Private Function HasContent2(l As IEnumerable(Of String)) As Boolean
            Return Enumerable.Count(l) >= &HE1 ' FN {{Hexadecimals are not picked up.}}
        End Function

        Private Function HasContent2b(l As IEnumerable(Of String)) As Boolean
            Return 1UL <= Enumerable.Count(l) ' Noncompliant
        End Function

        Private Function HasContent3(l As IEnumerable(Of String)) As Boolean
            Return l.Any()
        End Function

        Private Function IsNotEmpty1(l As IEnumerable(Of String)) As Boolean
            Return l.Count() <> 0 ' Noncompliant
        End Function

        Private Function IsNotEmpty2(l As IEnumerable(Of String)) As Boolean
            Return 0 <> l.Count() ' Noncompliant As Boolean
        End Function

        Private Function IsEmpty1(l As IEnumerable(Of String)) As Boolean
            Return l.Count() = 0 ' Noncompliant
        End Function

        Private Function IsEmpty2(l As IEnumerable(Of String)) As Boolean
            Return l.Count() <= 0 ' Noncompliant
        End Function

        Private Function IsEmpty2b(l As IEnumerable(Of String)) As Boolean
            Return 0 >= l.Count() ' Noncompliant
        End Function

        Private Function IsEmpty3(l As IEnumerable(Of String)) As Boolean
            Return Not l.Any() 'Compliant
        End Function

        Private Function IsEmpty4(l As IEnumerable(Of String)) As Boolean
            Return l.Count() < 1 ' Noncompliant
        End Function

        Private Function IsEmpty4b(l As IEnumerable(Of String)) As Boolean
            Return 1 > l.Count() ' Noncompliant
        End Function

        Private Function HasContentWithCondition(numbers As IEnumerable(Of Integer)) As Boolean
            Return numbers.Count(AddressOf IsNegative) > 0 ' Noncompliant
        End Function

        Private Function IsEmptyWithCondition(numbers As IEnumerable(Of Integer)) As Boolean
            Return numbers.Count(AddressOf IsNegative) = 0 ' Noncompliant
        End Function

        Private Function SizeDepedentCheck(numbers As Integer()) As Boolean
            Return numbers.Count() <> 2 ' Compliant 
        End Function

        Private Shared Function IsNegative(n As Integer) As Boolean
            Return n < 0
        End Function
    End Class

End Namespace
