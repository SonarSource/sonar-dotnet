Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq

Module Program
    Private list As New List(Of Integer)

    Sub Main()
    End Sub

    Private Sub SimpleCase_OrderBy()
        list.OrderBy(Function(x) x).Where(Function(x) True)
        '                           ^^^^^ {{"Where" should be used before "OrderBy"}}
        '    ^^^^^^^ Secondary@-1 {{This "OrderBy" precedes a "Where" - you should change the order.}}

        list.OrderBy(Function(x) x)?.Where(Function(x) True)
        '                            ^^^^^ {{"Where" should be used before "OrderBy"}}
        '    ^^^^^^^ Secondary@-1

        list?.OrderBy(Function(x) x).Where(Function(x) True)
        '                            ^^^^^ {{"Where" should be used before "OrderBy"}}
        '     ^^^^^^^ Secondary@-1

        list?.OrderBy(Function(x) x)?.Where(Function(x) True)
        '                             ^^^^^ {{"Where" should be used before "OrderBy"}}
        '     ^^^^^^^ Secondary@-1
    End Sub

    Private Sub SimpleCase_OrderByDescending()
        list.OrderByDescending(Function(x) x).Where(Function(x) True)
        '                                     ^^^^^ {{"Where" should be used before "OrderByDescending"}}
        '    ^^^^^^^^^^^^^^^^^ Secondary@-1 {{This "OrderByDescending" precedes a "Where" - you should change the order.}}

        list.OrderByDescending(Function(x) x)?.Where(Function(x) True)
        '                                      ^^^^^ {{"Where" should be used before "OrderByDescending"}}
        '    ^^^^^^^^^^^^^^^^^ Secondary@-1

        list?.OrderByDescending(Function(x) x).Where(Function(x) True)
        '                                      ^^^^^ {{"Where" should be used before "OrderByDescending"}}
        '     ^^^^^^^^^^^^^^^^^ Secondary@-1

        list?.OrderByDescending(Function(x) x)?.Where(Function(x) True)
        '                                       ^^^^^ {{"Where" should be used before "OrderByDescending"}}
        '     ^^^^^^^^^^^^^^^^^ Secondary@-1
    End Sub

    Private Sub OrderWhereWhere()
        list.OrderBy(Function(x) x).Where(Function(x) True).Where(Function(x) False)
        '                           ^^^^^ {{"Where" should be used before "OrderBy"}}
        '    ^^^^^^^ Secondary@-1

        list.OrderBy(Function(x) x)?.Where(Function(x) True)?.Where(Function(x) False)
        '                            ^^^^^ {{"Where" should be used before "OrderBy"}}
        '    ^^^^^^^ Secondary@-1

        list?.OrderBy(Function(x) x).Where(Function(x) True).Where(Function(x) False)
        '                            ^^^^^ {{"Where" should be used before "OrderBy"}}
        '     ^^^^^^^ Secondary@-1

        list?.OrderBy(Function(x) x)?.Where(Function(x) True)?.Where(Function(x) False)
        '                             ^^^^^ {{"Where" should be used before "OrderBy"}}
        '     ^^^^^^^ Secondary@-1
    End Sub

    Private Sub CompliantCases()
        list.OrderBy(Function(x) x).[Select](Function(x) x).Where(Function(x) True) ' Compliant
        Dim ordered = list.OrderBy(Function(x) x)
        ordered.Where(Function(x) True) ' Compliant

        list.OrderBy(Function(x) x).Where(Function(element, index) element >= index) ' Compliant
        list.OrderByDescending(Function(x) x).Where(Function(element, index) element >= index) ' Compliant
        list.OrderBy(Function(x) x)?.Where(Function(element, index) element >= index) ' Compliant
        list.OrderBy(Function(x) x).Where(Function(element, unusedIndex) element > 0) ' Compliant
        list.OrderByDescending(Function(x) x).Where(AddressOf HasEvenIndex) ' Compliant

        Dim fake = New Fake(Of Integer)()
        fake.OrderBy(Function(x) x).Where(Function(x) True) ' Compliant

        Dim semiFake = New SemiFake(Of Integer)()
        ' "Where" is the LINQ version, "OrderBy" is custom extension
        semiFake.OrderBy(Function(x) x).Where(Function(x) True) ' Compliant
    End Sub

    Private Function HasEvenIndex(element As Integer, index As Integer) As Boolean
        Return index Mod 2 = 0
    End Function

    Private Sub CustomImplementation()
        Dim mine = New MyEnumerable(Of Integer)()

        mine.OrderBy(Function(x) x).Where(Function(x) True)
        '                           ^^^^^ {{"Where" should be used before "OrderBy"}}
        '    ^^^^^^^ Secondary@-1

        mine?.OrderByDescending(Function(x) x)?.Where(Function(x) True)
        '                                       ^^^^^ {{"Where" should be used before "OrderByDescending"}}
        '     ^^^^^^^^^^^^^^^^^ Secondary@-1
    End Sub

End Module

Public Class MyEnumerable(Of T)
    Implements IEnumerable(Of T)

    Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Return Nothing
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return Nothing
    End Function
End Class

Public Class Fake(Of T)
End Class

Public Class SemiFake(Of T)
    Implements IEnumerable(Of T)

    Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Return Nothing
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return Nothing
    End Function
End Class

Module FakeExtensions
    <System.Runtime.CompilerServices.Extension()>
    Public Function Where(Of TSource)(ByVal source As Fake(Of TSource), ByVal predicate As Func(Of TSource, Boolean)) As Fake(Of TSource)
        Return source
    End Function

    <System.Runtime.CompilerServices.Extension()>
    Public Function OrderBy(Of TSource, TKey)(ByVal source As Fake(Of TSource), ByVal keySelector As Func(Of TSource, TKey)) As Fake(Of TSource)
        Return source
    End Function

    <System.Runtime.CompilerServices.Extension()>
    Public Function OrderBy(Of TSource, TKey)(ByVal source As SemiFake(Of TSource), ByVal keySelector As Func(Of TSource, TKey)) As SemiFake(Of TSource)
        Return source
    End Function
End Module
