Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Linq

Public Class Programs
    Private Sub GetOrAdd(ByVal dictionary As ConcurrentDictionary(Of Integer, Integer), ByVal key As Integer)
        ' GetOrAdd(TKey, Func<TKey,TValue>)
        dictionary.GetOrAdd(key, Function(__) key + 42) ' Noncompliant {{Use the lambda parameter instead of capturing the argument 'key'}}
        '                        ^^^^^^^^^^^^^^^^^^^^^
        dictionary.GetOrAdd(key, Function(__) key) ' Noncompliant

        dictionary.GetOrAdd(42, Function(__) key + 42)
        dictionary.GetOrAdd(key, Function(key) key + 42) ' Error [BC36641]
        ' Noncompliant@-1
        dictionary.GetOrAdd(42, Function(key) key + 42)  ' Error [BC36641]

        ' GetOrAdd(TKey, TValue)
        dictionary.GetOrAdd(42, 42)
        dictionary.GetOrAdd(key, 42)
        dictionary.GetOrAdd(key, key)

        ' GetOrAdd<TArg>(TKey, Func<TKey,TArg,TValue>, TArg)
        dictionary.GetOrAdd(key, Function(__, arg) key + 42, 42)   ' Noncompliant
        '                        ^^^^^^^^^^^^^^^^^^^^^^^^^^
        dictionary.GetOrAdd(key, Function(__, arg) arg + 42, 42)
        dictionary.GetOrAdd(42, Function(key, arg) arg + 42, 42)  ' Error [BC36641]
        dictionary.GetOrAdd(key, Function(key, arg) arg + 42, 42) ' Error [BC36641]
    End Sub

    Private Sub AddOrUpdate(ByVal dictionary As ConcurrentDictionary(Of Integer, Integer), ByVal key As Integer)
        ' AddOrUpdate(TKey, Func<TKey,TValue>, Func<TKey,TValue,TValue>)
        dictionary.AddOrUpdate(key, Function(__) key, Function(__, oldValue) oldValue + 42) ' Noncompliant
        '                           ^^^^^^^^^^^^^^^^
        dictionary.AddOrUpdate(key, Function(__) key, Function(__, oldValue) key + 42)
        '                           ^^^^^^^^^^^^^^^^ {{Use the lambda parameter instead of capturing the argument 'key'}}
        '                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1 {{Use the lambda parameter instead of capturing the argument 'key'}}
        dictionary.AddOrUpdate(key, Function(__) 42, Function(__, oldValue) oldValue + 42)
        dictionary.AddOrUpdate(42, Function(__) 42, Function(__, oldValue) oldValue + 42)
        dictionary.AddOrUpdate(42, Function(__) 42, Function(key, oldValue) key + 42) ' Error [BC36641]

        ' AddOrUpdate(TKey, TValue, Func<TKey,TValue,TValue>)
        dictionary.AddOrUpdate(key, 42, Function(__, oldValue) key + 42) ' Noncompliant
        dictionary.AddOrUpdate(42, key, Function(__, oldValue) key + 42)
        dictionary.AddOrUpdate(42, 42, Function(__, oldValue) key + 42)
        dictionary.AddOrUpdate(key, key, Function(__, oldValue) oldValue + 42)
        dictionary.AddOrUpdate(42, 42, Function(key, oldValue) key + 42) ' Error [BC36641]
        dictionary.AddOrUpdate(42, 42, Function(__, oldValue) oldValue + 42)

        ' AddOrUpdate<TArg>(TKey, Func<TKey,TArg,TValue>, Func<TKey,TValue,TArg,TValue>, TArg)
        dictionary.AddOrUpdate(key, Function(__, arg) key, Function(__, oldValue, arg) oldValue + arg, 42) ' Noncompliant
        dictionary.AddOrUpdate(key, Function(__, arg) key, Function(__, oldValue, arg) oldValue + key + arg, 42)
        '                           ^^^^^^^^^^^^^^^^^^^^^
        '                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1
        dictionary.AddOrUpdate(key, Function(__, arg) arg, Function(__, oldValue, arg) oldValue + arg, 42)
        dictionary.AddOrUpdate(42, Function(__, arg) key, Function(__, oldValue, arg) oldValue + arg, 42)
        dictionary.AddOrUpdate(42, Function(__, arg) key, Function(__, oldValue, arg) oldValue + arg, 42)
        dictionary.AddOrUpdate(42, Function(__, arg) arg, Function(__, oldValue, arg) oldValue + arg, key)
        dictionary.AddOrUpdate(42, Function(__, arg) key, Function(__, oldValue, arg) oldValue + arg, key)
        dictionary.AddOrUpdate(key, Function(__, arg) key, Function(__, key, arg) arg, key) ' Error [BC36641]
        ' Noncompliant@-1
        dictionary.AddOrUpdate(42, Function(__, arg) key, Function(__, key, arg) key + arg, key) ' Error [BC36641]
    End Sub

    Private Sub CompliantInvocations(ByVal dictionary As ConcurrentDictionary(Of Integer, Integer), ByVal hidesMethod As HidesMethod(Of Integer, Integer), ByVal list As List(Of Integer), ByVal key As Integer)
        dictionary.TryAdd(key, 42) ' Compliant
        list.Any(Function(x) key > 0) ' Compliant
        hidesMethod.GetOrAdd(key, Function(__) key) ' Compliant
    End Sub

    Private Sub MyDictionary(ByVal dictionary As MyConcurrentDictionary, ByVal key As Integer)
        dictionary.GetOrAdd(key, Function(__) key + 42) ' Noncompliant
    End Sub

    Private Sub [NameOf](ByVal dictionary As ConcurrentDictionary(Of String, String), ByVal key As String, ByVal str As String)
        dictionary.GetOrAdd(key, Function(__) NameOf(key)) ' Compliant
        dictionary.GetOrAdd(key, Function(x)
                                     Dim something = $"The name should be {NameOf(key)} and not {NameOf(x)}" ' Compliant
                                     Return x
                                 End Function)
    End Sub

    Class MyConcurrentDictionary
        Inherits ConcurrentDictionary(Of Integer, Integer)
    End Class

    Class HidesMethod(Of TKey, TValue)
        Inherits ConcurrentDictionary(Of TKey, TValue)

        Public Function GetOrAdd(ByVal key As TKey, ByVal valueFactory As Func(Of TKey, TValue)) As TValue
            Return Nothing
        End Function
    End Class
End Class
