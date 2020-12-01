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
            Const localConst_Zero As Integer = 0
            Const localConst_NonZero As Integer = 1
            Dim localVariable As Integer = 0
            Dim result As Boolean

            Dim someEnumerable As IEnumerable(Of String) = New List(Of String)()

            result = Enumerable.Count(someEnumerable) >= 0 ' Noncompliant {{The count of 'IEnumerable(Of T)' is always '>=0', so fix this test to get the real expected behavior.}}
'                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            result = someEnumerable.Count(Function(foo) True) >= 0 ' Noncompliant
            result = someEnumerable?.Count() >= 0 ' Noncompliant
            result = someEnumerable.Count() >= 1
            result = someEnumerable.Count() >= localVariable
            result = someEnumerable.Count() >= -1
            result = someEnumerable.Count() <= 0
            result = someEnumerable.Count() < 0
            result = 0 >= someEnumerable.Count()

            result = someEnumerable.Count() >= localConst_Zero ' Noncompliant
            result = someEnumerable.Count() >= ConstField_NonZero
            result = someEnumerable.Count() >= ConstField_Zero ' Noncompliant
            result = someEnumerable.Count() >= localConst_NonZero

            result = (someEnumerable.Count()) >= (0) ' Noncompliant
            result = ((((someEnumerable).Count())) >= ((0))) ' Noncompliant
'                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            result = 0 <= someEnumerable.Count() ' Noncompliant
'                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim nonEnumerable = New FooMethod()
            result = nonEnumerable.Count() >= 0
        End Sub

        Public Sub TestComplexAccess()
            Dim someArray = New String(-1) {}
            Dim someEnumerable = New List(Of String)()
            Dim holder = New DummyHolder()

            Dim result As Boolean
            result = holder.GetHolder().GetHolder().GetArray().Count() >= 0 ' Noncompliant
            result = holder.GetHolder()?.GetHolder()?.GetArray()?.Length >= 0 ' Noncompliant
            result = holder.GetHolder()?.GetHolder().Holder.Array.Length >= 0 ' Noncompliant
            result = holder.GetHolder().GetHolder().Holder.Enumerable.AsEnumerable.Count(Function(foo) True) >= 0 ' Noncompliant
            result = (holder.GetHolder()?.GetHolder())?.GetArray()?.Length >= 0 ' Noncompliant
        End Sub

        Public Sub TestCountProperty()
            Dim someCollection = New List(Of String)()
            Dim result As Boolean = someCollection.Count >= 0 ' Noncompliant {{The count of 'ICollection' is always '>=0', so fix this test to get the real expected behavior.}}
'                                   ^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim nonCollection = New FooProperty()
            result = nonCollection.Count >= 0
        End Sub

        Public Sub TestLengthProperty()
            Dim someArray = New String(-1) {}
            Dim result As Boolean

            result = someArray.Length >= 0 ' Noncompliant {{The length of 'Array' is always '>=0', so fix this test to get the real expected behavior.}}
'                    ^^^^^^^^^^^^^^^^^^^^^
            result = someArray.LongLength >= 0 ' Noncompliant {{The longlength of 'Array' is always '>=0', so fix this test to get the real expected behavior.}}
'                    ^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim nonArray = New FooProperty()
            result = nonArray.Length >= 0
            result = nonArray.LongLength >= 0
        End Sub
    End Class
End Namespace
