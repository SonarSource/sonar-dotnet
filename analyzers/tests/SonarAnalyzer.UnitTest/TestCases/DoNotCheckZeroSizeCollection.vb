Imports System.Collections.Generic
Imports System.Linq

Namespace Tests.Diagnostics
    Class FooProperty
        Public ReadOnly Property Length() As Integer
            Get
                Return 0
            End Get
        End Property

        Public ReadOnly Property LongLength() As Integer
            Get
                Return 0
            End Get
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Return 0
            End Get
        End Property
    End Class

    Class FooMethod
        Public Function Count() As Integer
            Return 0
        End Function
    End Class

    Class DummyHolder
        Public Enumerable As List(Of String)
        Public Function GetEnumerable() As List(Of String)
            Return Nothing
        End Function

        Public Array As String()
        Public Function GetArray() As String()
            Return Nothing
        End Function

        Public Holder As DummyHolder
        Public Function GetHolder() As DummyHolder
            Return Nothing
        End Function
    End Class

    Class Program
        Const ConstField_Zero As Integer = 0
        Const ConstField_NonZero As Integer = 1

        Public Sub TestCountMethod()
            Const LocalConst_Zero As Integer = 0
            Const LocalConst_NonZero As Integer = 1
            Dim LocalVariable As Integer = 0
            Dim Result As Boolean

            Dim SomeEnumerable As IEnumerable(Of String) = New List(Of String)()

            Result = Enumerable.Count(SomeEnumerable) >= 0 ' Noncompliant {{The 'Count' of 'IEnumerable(Of T)' is always '>=0', so fix this test to get the real expected behavior.}}
'                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Result = SomeEnumerable.CoUnT(Function(foo) True) >= 0 ' Noncompliant {{The 'Count' of 'IEnumerable(Of T)' is always '>=0', so fix this test to get the real expected behavior.}}
            Result = SomeEnumerable?.Count() >= 0 ' Noncompliant
            Result = SomeEnumerable.Count() >= 1
            Result = SomeEnumerable.Count() >= localVariable
            Result = SomeEnumerable.Count() >= -1
            Result = SomeEnumerable.Count() <= 0
            Result = SomeEnumerable.Count() < 0
            Result = 0 >= SomeEnumerable.Count()

            Result = SomeEnumerable.Count() >= LocalConst_Zero ' Noncompliant
            Result = SomeEnumerable.Count() >= ConstField_NonZero
            Result = SomeEnumerable.Count() >= ConstField_Zero ' Noncompliant
            Result = SomeEnumerable.Count() >= LocalConst_NonZero

            Result = (SomeEnumerable.Count()) >= (0) ' Noncompliant
            Result = ((((SomeEnumerable).Count())) >= ((0))) ' Noncompliant
'                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Result = 0 <= SomeEnumerable.Count() ' Noncompliant
'                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim NonEnumerable = New FooMethod()
            Result = NonEnumerable.Count() >= 0
        End Sub

        Public Sub TestComplexAccess()
            Dim SomeArray = New String(-1) {}
            Dim SomeEnumerable = New List(Of String)()
            Dim Holder = New DummyHolder()

            Dim Result As Boolean
            Result = Holder.GetHolder().GetHolder().GetArray().Count() >= 0 ' Noncompliant
            Result = Holder.GetHolder()?.GetHolder()?.GetArray()?.Length >= 0 ' Noncompliant
            Result = Holder.GetHolder()?.GetHolder().Holder.Array.Length >= 0 ' Noncompliant
            Result = Holder.GetHolder().GetHolder().Holder.Enumerable.AsEnumerable.Count(Function(foo) True) >= 0 ' Noncompliant
            Result = (Holder.GetHolder()?.GetHolder())?.GetArray()?.Length >= 0 ' Noncompliant
        End Sub

        Public Sub TestCountProperty()
            Dim SomeCollection = New List(Of String)()
            Dim Result As Boolean = SomeCollection.Count >= 0 ' Noncompliant {{The 'Count' of 'ICollection' is always '>=0', so fix this test to get the real expected behavior.}}
'                                   ^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim NonCollection = New FooProperty()
            Result = NonCollection.Count >= 0
        End Sub

        Public Sub TestLengthProperty()
            Dim SomeArray = New String(-1) {}
            Dim Result As Boolean

            Result = SomeArray.Length >= 0 ' Noncompliant {{The 'Length' of 'Array' is always '>=0', so fix this test to get the real expected behavior.}}
'                    ^^^^^^^^^^^^^^^^^^^^^
            Result = SomeArray.LongLength >= 0 ' Noncompliant {{The 'LongLength' of 'Array' is always '>=0', so fix this test to get the real expected behavior.}}
'                    ^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim NonArray = New FooProperty()
            Result = NonArray.Length >= 0
            Result = NonArray.LongLength >= 0
        End Sub

        Public Sub TestInterfacesAndReadonlyCollections(List As IList(Of Integer), Collection As ICollection(Of Integer), ReadonlyCollection As IReadOnlyCollection(Of Integer), ReadonlyList As IReadOnlyList(Of Integer))
            Dim Result As Boolean
            Dim SortedSet As New SortedSet(Of Double)

            Result = List.Count >= 0 ' Noncompliant

            Result = Collection.Count >= 0 ' Noncompliant

            Result = ReadonlyCollection.Count >= 0 ' Noncompliant

            Result = ReadonlyList.Count >= 0 ' Noncompliant

            Result = SortedSet.Count >= 0 ' Noncompliant
        End Sub

    End Class
End Namespace
