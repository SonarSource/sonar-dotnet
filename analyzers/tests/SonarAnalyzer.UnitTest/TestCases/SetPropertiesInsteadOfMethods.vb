Imports System
Imports System.Linq
Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.Immutable

Module Program
    Sub SortedSet()
        Dim sortedSet = New SortedSet(Of Integer)()

        Dim dummy = Enumerable.Min(sortedSet) ' Noncompliant {{"Min" property of Set type should be used instead of the "Min()" extension method.}}
        dummy = Enumerable.Max(sortedSet) ' Noncompliant {{"Max" property of Set type should be used instead of the "Max()" extension method.}}

        Dim funcMin As Func(Of SortedSet(Of Integer), Integer) = Function(x) Enumerable.Min(x) ' Noncompliant
        Dim funcMax As Func(Of SortedSet(Of Integer), Integer) = Function(x) Enumerable.Max(x) ' Noncompliant

        Dim doWork As Func(Of SortedSet(Of Integer)) = Function() Nothing

        dummy = Enumerable.Min(doWork()) ' Noncompliant
        dummy = Enumerable.Min(doWork.Invoke()) ' Noncompliant

        dummy = Enumerable.Min(New SortedSet(Of Integer)({42})) ' Noncompliant
        dummy = Enumerable.Max(New SortedSet(Of Integer)({42})) ' Noncompliant

        dummy = Enumerable.Min(sortedSet, Function(x) 42) ' Compliant, predicate used
        dummy = Enumerable.Max(sortedSet, Function(x) 42.0F) ' Compliant, predicate used
    End Sub

    Sub DerivesFromSortedSet()
        Dim sortedSetDerived = New DerivesFromSetType(Of Integer)()

        Dim ternary = IIf(True, sortedSetDerived, sortedSetDerived)
        Enumerable.Min(ternary) ' Compliant - IIf returns un-typed object
        Enumerable.Max(ternary) ' Compliant - IIf returns untyped object

        Enumerable.Min(sortedSetDerived.Fluent().Fluent().Fluent().Fluent()) ' Noncompliant
        Enumerable.Max(sortedSetDerived.Fluent().Fluent().Fluent()?.Fluent()) ' Noncompliant
        Enumerable.Min(sortedSetDerived.Fluent().Fluent()?.Fluent().Fluent()) ' Noncompliant
        Enumerable.Max(sortedSetDerived.Fluent().Fluent()?.Fluent()?.Fluent()) ' Noncompliant
        Enumerable.Min(sortedSetDerived.Fluent()?.Fluent().Fluent().Fluent()) ' Noncompliant
        Enumerable.Max(sortedSetDerived.Fluent()?.Fluent().Fluent()?.Fluent()) ' Noncompliant
        Enumerable.Min(sortedSetDerived.Fluent()?.Fluent()?.Fluent().Fluent()) ' Noncompliant
        Enumerable.Max(sortedSetDerived.Fluent()?.Fluent()?.Fluent()?.Fluent()) ' Noncompliant
        '          ^^^
        Enumerable.Min(sortedSetDerived, Function(x) x = 42) ' Compliant, predicate used
        Enumerable.Max(sortedSetDerived, Function(x) x = 42.0F) ' Compliant, predicate used
    End Sub

    Sub TrueNegatives()
        Dim sortedSet = New SortedSet(Of Integer)()
        Dim dummy = sortedSet.Min ' Compliant
        dummy = sortedSet.Max ' Compliant
        dummy = sortedSet.Min() ' Compliant
        dummy = sortedSet.Max() ' Compliant

        Dim doesNotDerive = New DoesNotDeriveFromSetType(Of Integer)()
        doesNotDerive.Min() ' Compliant, does not derive from Set type
        doesNotDerive.Max() ' Compliant, does not derive from Set type
        dummy = Enumerable.Min(doesNotDerive) ' Compliant, does not derive from Set type
        dummy = Enumerable.Max(doesNotDerive) ' Compliant, does not derive from Set type
    End Sub
End Module

Class DerivesFromSetType(Of T)
    Inherits SortedSet(Of T)

    Public Function Fluent() As DerivesFromSetType(Of T)
        Return Me
    End Function
End Class

Class DoesNotDeriveFromSetType(Of T)
    Implements IEnumerable(Of T)

    Public Function Min() As Integer
        Return 42
    End Function

    Public Function Max() As Integer
        Return 42
    End Function

    Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Return Nothing
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return Nothing
    End Function
End Class
