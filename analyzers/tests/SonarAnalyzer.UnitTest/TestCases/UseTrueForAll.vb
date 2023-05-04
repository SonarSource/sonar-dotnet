Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection

Class ListTestcases
    Private Sub Playground()
        Dim list = New List(Of Integer)()

        Dim bad = list.All(Function(x) True) ' Noncompliant
        Dim nullableBad = list?.All(Function(x) True) ' Noncompliant

        Dim good = list.TrueForAll(Function(x) True) ' Compliant

        Dim DoWork As Func(Of List(Of Integer)) = Function() Nothing
        Dim func As Func(Of List(Of Integer), Boolean) = Function(l) l.All(Function(x) True) ' Noncompliant

        Dim methodBad = DoWork().All(Function(x) True) ' Noncompliant
        Dim methodGood = DoWork().TrueForAll(Function(x) True) ' Compliant

        Dim nullableMethodBad = DoWork()?.All(Function(x) True) ' Noncompliant
        Dim nullableMethodGood = DoWork()?.TrueForAll(Function(x) True) ' Compliant

        Dim inlineInitialization = New List(Of Integer) From {42}.All(Function(x) True) ' Noncompliant

        Dim imposter = New ContainsAllMethod(Of Integer)()
        imposter.All(Function(x) True) ' Compliant

        Dim badList = New BadList(Of Integer)()
        badList.All(Function(x) True) ' Compliant

        Dim goodList = New GoodList(Of Integer)()
        goodList.All(Function(x) True) ' Noncompliant

        Dim ternary As List(Of Integer) = If(True, list, goodList)
        ternary.All(Function(x) True) 'Noncompliant

        goodList.Fluent().Fluent().Fluent().Fluent().All(Function(x) True) 'Noncompliant
        goodList.Fluent().Fluent().Fluent().Fluent()?.All(Function(x) True) 'Noncompliant
        goodList.Fluent().Fluent().Fluent()?.Fluent().All(Function(x) True) 'Noncompliant
        goodList.Fluent().Fluent().Fluent()?.Fluent()?.All(Function(x) True) 'Noncompliant
        goodList.Fluent().Fluent()?.Fluent().Fluent().All(Function(x) True) 'Noncompliant
        goodList.Fluent().Fluent()?.Fluent().Fluent()?.All(Function(x) True) 'Noncompliant
        goodList.Fluent().Fluent()?.Fluent()?.Fluent().All(Function(x) True) 'Noncompliant
        goodList.Fluent().Fluent()?.Fluent()?.Fluent()?.All(Function(x) True) 'Noncompliant
        goodList.Fluent()?.Fluent().Fluent().Fluent().All(Function(x) True) 'Noncompliant
        goodList.Fluent()?.Fluent().Fluent().Fluent()?.All(Function(x) True) 'Noncompliant
        goodList.Fluent()?.Fluent().Fluent()?.Fluent().All(Function(x) True) 'Noncompliant
        goodList.Fluent()?.Fluent().Fluent()?.Fluent()?.All(Function(x) True) 'Noncompliant
        goodList.Fluent()?.Fluent()?.Fluent().Fluent().All(Function(x) True) 'Noncompliant
        goodList.Fluent()?.Fluent()?.Fluent().Fluent()?.All(Function(x) True) 'Noncompliant
        goodList.Fluent()?.Fluent()?.Fluent()?.Fluent().All(Function(x) True) 'Noncompliant
    End Sub

    Class GoodList(Of T)
        Inherits List(Of T)

        Public Function Fluent() As GoodList(Of T)
            Return Me
        End Function
    End Class

    Class BadList(Of T)
        Inherits List(Of T)

        Public Function All(predicate As Func(Of T, Boolean)) As Boolean
            Return True
        End Function
    End Class

    Class ContainsAllMethod(Of T)
        Public Function All(predicate As Func(Of T, Boolean)) As Boolean
            Return True
        End Function
    End Class
End Class

Class ArrayTestcases
    Private Sub Playground()
        Dim array = New Integer(41) {}

        Dim bad = array.All(Function(x) True) ' Noncompliant
        Dim nullableBad = array?.All(Function(x) True) ' Noncompliant

        Dim good = array.TrueForAll(array, Function(x) True) ' Compliant

        Dim func As Func(Of Integer(), Boolean) = Function(l) l.All(Function(x) True) ' Noncompliant

        Dim methodBad = DoWork().All(Function(x) True) ' Noncompliant
        Dim methodGood = array.TrueForAll(DoWork(), Function(x) True) ' Compliant

        Dim nullableMethodBad = DoWork()?.All(Function(x) True) ' Noncompliant
        Dim inlineInitialization = {42}.All(Function(x) True) ' FN `.GetTypeInfo(CollectionInitializer)` returns null

        array.ToArray().ToArray().ToArray().ToArray().All(Function(x) True) 'Noncompliant
        array.ToArray().ToArray().ToArray().ToArray()?.All(Function(x) True) 'Noncompliant
        array.ToArray().ToArray().ToArray()?.ToArray().All(Function(x) True) 'Noncompliant
        array.ToArray().ToArray().ToArray()?.ToArray()?.All(Function(x) True) 'Noncompliant
        array.ToArray().ToArray()?.ToArray().ToArray().All(Function(x) True) 'Noncompliant
        array.ToArray().ToArray()?.ToArray().ToArray()?.All(Function(x) True) 'Noncompliant
        array.ToArray().ToArray()?.ToArray()?.ToArray().All(Function(x) True) 'Noncompliant
        array.ToArray().ToArray()?.ToArray()?.ToArray()?.All(Function(x) True) 'Noncompliant
        array.ToArray()?.ToArray().ToArray().ToArray().All(Function(x) True) 'Noncompliant
        array.ToArray()?.ToArray().ToArray().ToArray()?.All(Function(x) True) 'Noncompliant
        array.ToArray()?.ToArray().ToArray()?.ToArray().All(Function(x) True) 'Noncompliant
        array.ToArray()?.ToArray().ToArray()?.ToArray()?.All(Function(x) True) 'Noncompliant
        array.ToArray()?.ToArray()?.ToArray().ToArray().All(Function(x) True) 'Noncompliant
        array.ToArray()?.ToArray()?.ToArray().ToArray()?.All(Function(x) True) 'Noncompliant
        array.ToArray()?.ToArray()?.ToArray()?.ToArray().All(Function(x) True) 'Noncompliant
        array.ToArray()?.ToArray()?.ToArray()?.ToArray()?.All(Function(x) True) 'Noncompliant
    End Sub

    Private Function DoWork() As Integer()
        Return Nothing
    End Function
End Class

