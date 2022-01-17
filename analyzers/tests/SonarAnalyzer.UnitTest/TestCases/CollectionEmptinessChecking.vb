Imports System.Collections.Generic
Imports System.Linq

Namespace Tests.Diagnostics

    Public Class CollectionEmptinessChecking

        Private Function HasContent1(l As IEnumerable(Of String)) As Boolean
            Return l.Count() > 0 ' Noncompliant {{Use '.Any()' to test whether this 'IEnumerable(Of String)' is empty or not.}}
            '        ^^^^^
        End Function

        Private Function HasContent1b(l As IEnumerable(Of String)) As Boolean
            Return 0 < l.Count() ' Noncompliant
        End Function

        Private Function HasContent2(l As IEnumerable(Of String)) As Boolean
            Return Enumerable.Count(l) >= &HE1 ' FN. Hexadecimals are not picked up.
        End Function

        Private Function HasContent2b(l As IEnumerable(Of String)) As Boolean
            Return 1UL <= Enumerable.Count(l) ' Noncompliant
        End Function

        Private Function IsNotEmpty1(l As IEnumerable(Of String)) As Boolean
            Return l.Count() <> 0 ' Noncompliant
        End Function

        Private Function IsNotEmpty2(l As IEnumerable(Of String)) As Boolean
            Return 0 <> l.Count() ' Noncompliant
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

    Public Class Compliant

        Function Any(list As List(Of String)) As Boolean
            Return list.Any()
        End Function

        Function NotAny(list As List(Of String)) As Boolean
            Return Not list.Any()
        End Function

        Function NotAnExtension(model As Compliant) As Boolean
            Return model.Count() = 0
        End Function

        Function NotAMethod(value As Integer) As Boolean
            Return value <> 0
        End Function

        Function SizeDepedent(numbers As Integer()) As Boolean
            Return numbers.Count() <> 2 OrElse
                numbers.Count(AddressOf IsNegative) = 3 OrElse
                1 <> numbers.Count() OrElse
                1 = numbers.Count(AddressOf IsNegative) OrElse
                Enumerable.Count(numbers) > 1 OrElse
                42 < Enumerable.Count(numbers)
        End Function

        Function Undefined(model As Date) As Boolean
            Return model.Count() ' Error[BC30456]
        End Function

        Function Count() As Integer
            Return 42
        End Function

        Shared Function IsNegative(n As Integer) As Boolean
            Return n < 0
        End Function

    End Class

End Namespace
