Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq

Module Module1
    Sub Main()
        Dim list As New List(Of Integer)()

        Dim dummy = list(0) ' Compliant
        dummy = list(list.Count - 1) ' Compliant
        dummy = list(42) ' Compliant

        Dim badFirst = list.First() ' Noncompliant {{Indexing at 0 should be used instead of the "Enumerable" extension method "First"}}
        '                   ^^^^^
        Dim badLast = list.Last() ' Noncompliant {{Indexing at Count-1 should be used instead of the "Enumerable" extension method "Last"}}
        '                  ^^^^
        Dim badElementAt = list.ElementAt(42) ' Noncompliant {{Indexing should be used instead of the "Enumerable" extension method "ElementAt"}}
        '                       ^^^^^^^^^
        Dim badFirstNullable = list?.First() ' Noncompliant {{Indexing at 0 should be used instead of the "Enumerable" extension method "First"}}
        '                            ^^^^^
        Dim badLastNullable = list?.Last() ' Noncompliant {{Indexing at Count-1 should be used instead of the "Enumerable" extension method "Last"}}
        '                           ^^^^
        Dim badElementAtNullable = list?.ElementAt(42) ' Noncompliant {{Indexing should be used instead of the "Enumerable" extension method "ElementAt"}}
        '                                ^^^^^^^^^

        Dim func As Func(Of List(Of Integer), Integer) = Function(l) l.First() ' Noncompliant

        dummy = DoWork().First() ' Noncompliant
        dummy = DoWork().Last() ' Noncompliant
        dummy = DoWork().ElementAt(42) ' Noncompliant

        dummy = DoWork()?.First() ' Noncompliant
        dummy = DoWork()?.Last() ' Noncompliant
        dummy = DoWork()?.ElementAt(42) ' Noncompliant

        dummy = (New List(Of Integer)() From {42}).First() ' Noncompliant
        dummy = (New List(Of Integer)() From {42}).Last() ' Noncompliant
        dummy = (New List(Of Integer)() From {42}).ElementAt(42) ' Noncompliant

        Dim inlineInitialization = {42}.First() ' FN, .GetTypeInfo(CollectionInitializer) returns null
        inlineInitialization = {42}.Last() ' FN, .GetTypeInfo(CollectionInitializer) returns null
        inlineInitialization = {42}.ElementAt(42) ' FN, .GetTypeInfo(CollectionInitializer) returns null

        Dim implementsIList = New ImplementsIList(Of Integer)()

        implementsIList.Fluent().Fluent().Fluent().Fluent().First() ' Noncompliant
        implementsIList.Fluent().Fluent().Fluent().Fluent()?.Last() ' Noncompliant
        implementsIList.Fluent().Fluent().Fluent()?.Fluent().ElementAt(42) ' Noncompliant
        implementsIList.Fluent().Fluent().Fluent()?.Fluent()?.First() ' Noncompliant
        implementsIList.Fluent().Fluent()?.Fluent().Fluent().Last() ' Noncompliant
        implementsIList.Fluent().Fluent()?.Fluent().Fluent()?.ElementAt(42) ' Noncompliant
        implementsIList.Fluent().Fluent()?.Fluent()?.Fluent().First() ' Noncompliant
        implementsIList.Fluent().Fluent()?.Fluent()?.Fluent()?.Last() ' Noncompliant
        implementsIList.Fluent()?.Fluent().Fluent().Fluent().ElementAt(42) ' Noncompliant
        implementsIList.Fluent()?.Fluent().Fluent().Fluent()?.First() ' Noncompliant
        implementsIList.Fluent()?.Fluent().Fluent()?.Fluent().Last() ' Noncompliant
        implementsIList.Fluent()?.Fluent().Fluent()?.Fluent()?.ElementAt(42) ' Noncompliant
        implementsIList.Fluent()?.Fluent()?.Fluent().Fluent().First() ' Noncompliant
        implementsIList.Fluent()?.Fluent()?.Fluent().Fluent()?.Last() ' Noncompliant
        implementsIList.Fluent()?.Fluent()?.Fluent()?.Fluent().ElementAt(42) ' Noncompliant
        implementsIList.Fluent()?.Fluent()?.Fluent()?.Fluent()?.First() ' Noncompliant
        '                                                       ^^^^^

        implementsIList.First(Function(x) x = 42) ' Compliant, calls with the predicate cannot be replaced with indexes
        implementsIList.Last(Function(x) x = 42) ' Compliant, calls with the predicate cannot be replaced with indexes

        First(Of Integer)() ' Compliant
        Last(Of Integer)() ' Compliant
        ElementAt(Of Integer)(42) ' Compliant

        Dim fakeList As New FakeList(Of Integer)()

        fakeList.First() ' Compliant
        fakeList.Last() ' Compliant
        fakeList.ElementAt(42) ' Compliant

    End Sub

    Function DoWork() As List(Of Integer)
        Return Nothing
    End Function

    Function First(Of T)() As T
        Return Nothing
    End Function

    Function Last(Of T)() As T
        Return Nothing
    End Function

    Function ElementAt(Of T)(index As Integer) As T
        Return Nothing
    End Function

    Class ImplementsIList(Of T)
        Implements IList(Of T)

        Public Function Fluent() As ImplementsIList(Of T)
            Return Me
        End Function

        Default Public Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
            Get
                Return Nothing
            End Get
            Set(ByVal value As T)
                value = Nothing
            End Set
        End Property

        Public ReadOnly Property Count As Integer Implements IList(Of T).Count
            Get
                Return 42
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements IList(Of T).IsReadOnly
            Get
                Return True
            End Get
        End Property

        Public Sub Add(ByVal item As T) Implements IList(Of T).Add
        End Sub

        Public Sub Clear() Implements IList(Of T).Clear
        End Sub

        Public Function Contains(ByVal item As T) As Boolean Implements IList(Of T).Contains
            Return False
        End Function

        Public Sub CopyTo(ByVal array() As T, ByVal arrayIndex As Integer) Implements IList(Of T).CopyTo
        End Sub

        Public Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
            Return 42
        End Function

        Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
        End Sub

        Public Function Remove(ByVal item As T) As Boolean Implements IList(Of T).Remove
            Return False
        End Function

        Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
        End Sub

        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return Nothing
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Nothing
        End Function
    End Class

End Module

Class FakeList(Of T)
End Class

Module FakeListExtensions
    <System.Runtime.CompilerServices.Extension>
    Public Function First(Of TSource)(ByVal source As FakeList(Of TSource)) As TSource
        Return Nothing
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function Last(Of TSource)(ByVal source As FakeList(Of TSource)) As TSource
        Return Nothing
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function ElementAt(Of TSource)(ByVal source As FakeList(Of TSource), ByVal index As Integer) As TSource
        Return Nothing
    End Function
End Module

