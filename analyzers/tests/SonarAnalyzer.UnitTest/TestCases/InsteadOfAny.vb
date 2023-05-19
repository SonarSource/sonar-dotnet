Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq

Public Class TestClass
    Private Sub MyMethod(ByVal list As List(Of Integer), ByVal array As Integer())
        list.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        '    ^^^
        list.Any(Function(x) x = 0) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        '    ^^^

        list.Append(1).Any(Function(x) x > 1) ' Compliant (Appended list becomes an IEnumerable)
        list.Append(1).Append(2).Any(Function(x) x > 1) ' Compliant
        Enumerable.Any(Enumerable.Append(Enumerable.Append(list, 1), 2), Function(x) x > 1).ToString() ' Compliant 

        list.Any() ' Compliant (you can't use Exists with no arguments, CS7036)
        list.Exists(Function(x) x > 0) ' Compliant
        Dim a = list.Contains(0)

        array.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        array.Any() ' Compliant

        Dim classA = New ClassA()
        classA.myListField.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        classA.classB.myListField.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        classA.classB.myListField.Any() ' Compliant

        Dim classB = New ClassB()
        classB.Any(Function(x) x > 0) ' Compliant

        list?.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        classB?.Any(Function(x) x > 0) ' Compliant

        Dim del As Func(Of Integer, Boolean) = Function(x) True
        list.Any(del) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        Dim enumList = New EnumList(Of Integer)()
        enumList.Any(Function(x) x > 0) ' Compliant

        Dim goodList = New GoodList(Of Integer)()
        goodList.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        Dim ternary = If(True, list, goodList).Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        Dim nullCoalesce = If(list, goodList).Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        Dim ternaryNullCoalesce = If(list, If(True, list, goodList)).Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        goodList.GetList().Any(Function(x) True) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        Any(Of Integer)(Function(x) x > 0) ' Compliant
        Call AcceptMethod(New Func(Of Func(Of Integer, Boolean), Boolean)(AddressOf goodList.Any)) ' Compliant
    End Sub

    Private Sub List(ByVal pList As List(Of Integer))
        pList.Any(Function(x) x = 0) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        pList.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        pList.Any() ' Compliant
    End Sub

    Private Sub HashSet(ByVal pHashSet As HashSet(Of Integer))
        pHashSet.Any(Function(x) x = 0) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        pHashSet.Any(Function(x) x > 0) ' Compliant
        pHashSet.Any() ' Compliant
    End Sub

    Private Sub SortedSet(ByVal pSortedSet As SortedSet(Of Integer))
        pSortedSet.Any(Function(x) x = 0) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        pSortedSet.Any(Function(x) x > 0) ' Compliant
        pSortedSet.Any() ' Compliant
    End Sub

    Private Sub Array(ByVal pArray As Integer())
        pArray.Any(Function(x) x = 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        pArray.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        pArray.Any() ' Compliant
    End Sub

    Private Sub ConditionalsMatrix(ByVal goodList As GoodList(Of Integer))
        goodList.GetList().GetList().GetList().GetList().Any(Function(x) x > 0)     ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList().GetList().GetList().GetList()?.Any(Function(x) x > 0)    ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList().GetList().GetList()?.GetList().Any(Function(x) x > 0)    ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList().GetList().GetList()?.GetList()?.Any(Function(x) x > 0)   ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList().GetList()?.GetList().GetList().Any(Function(x) x > 0)    ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList().GetList()?.GetList().GetList()?.Any(Function(x) x > 0)   ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList().GetList()?.GetList()?.GetList().Any(Function(x) x > 0)   ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList().GetList()?.GetList()?.GetList()?.Any(Function(x) x > 0)  ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList()?.GetList().GetList().GetList().Any(Function(x) x > 0)    ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList()?.GetList().GetList().GetList()?.Any(Function(x) x > 0)   ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList()?.GetList().GetList()?.GetList().Any(Function(x) x > 0)   ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList()?.GetList().GetList()?.GetList()?.Any(Function(x) x > 0)  ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList()?.GetList()?.GetList().GetList().Any(Function(x) x > 0)   ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList()?.GetList()?.GetList().GetList()?.Any(Function(x) x > 0)  ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList()?.GetList()?.GetList()?.GetList().Any(Function(x) x > 0)  ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        goodList.GetList()?.GetList()?.GetList()?.GetList()?.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
    End Sub

    Private Sub CheckDelegate(ByVal intList As List(Of Integer), ByVal stringList As List(Of String), ByVal refList As List(Of ClassA), ByVal intArray As Integer(), ByVal someString As String, ByVal someInt As Integer, ByVal anotherInt As Integer, ByVal someRef As ClassA)
        intList.Any(Function(x) x = 0) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) 0 = x) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) x = someInt) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) someInt = x) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) x.Equals(0)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) 0.Equals(x)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}

        intList.Any(Function(x) x = x) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) someInt = anotherInt) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) someInt = 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) 0 = 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        intList.Any(Function(x) x.Equals(x)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) someInt.Equals(anotherInt)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) someInt.Equals(0)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) 0.Equals(0)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) x.Equals(x + 1)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        intList.Any(Function(x) x.GetType() Is GetType(Integer)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) x.GetType().Equals(GetType(Integer))) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}} FP
        intList.Any(Function(x) MyIntCheck(x)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) x <> 0)     ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) x.Equals(0) AndAlso True) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(Function(x) If(x = 0, 2, 0) = 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        stringList.Any(Function(x) Equals(x, "")) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) Equals("", x)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) Equals(x, someString)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) Equals(someString, x)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) x.Equals("")) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) "".Equals(x)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) Equals(x, "")) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) "" Is x) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) x Is "") ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) x Is Nothing) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}

        stringList.Any(Function(x) MyStringCheck(x)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) Not Equals(x, ""))     ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) x.Equals("") AndAlso True)   ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) Equals(If(Equals(x, ""), "a", "b"), "a")) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        stringList.Any(Function(x) x.Equals("" & someString)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        intArray.Any(Function(x) x = 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(Function(x) 0 = x) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(Function(x) x = someInt) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(Function(x) someInt = x) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(Function(x) x.Equals(0)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(Function(x) 0.Equals(x)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(Function(x) someInt.Equals(x)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(Function(x) x.Equals(x + 1)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        refList.Any(Function(x) x Is someRef) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        refList.Any(Function(x) someRef Is x) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        refList.Any(Function(x) x.Equals(someRef))  ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        refList.Any(Function(x) someRef.Equals(x))  ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        refList.Any(Function(x) Equals(someRef, x)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        refList.Any(Function(x) Equals(x, someRef)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}

        intList.Any(Function(x) x Is Nothing) ' Error [BC30020]
        intList.Any(Function(x) x.Equals(Nothing)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}} FP
        intList.Any(Function(x) Equals(x, Nothing)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}} FP

        refList.Any(Function(x) x = Nothing) ' Error [BC30452]
        refList.Any(Function(x) x.Equals(Nothing)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        refList.Any(Function(x) Equals(x, Nothing)) ' Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
    End Sub

    Private Function MyIntCheck(ByVal x As Integer) As Boolean
        Return x = 0
    End Function
    Private Function MyStringCheck(ByVal x As String) As Boolean
        Return Equals(x, "")
    End Function

    Private Function Any(Of T)(ByVal predicate As Func(Of T, Boolean)) As Boolean
        Return True
    End Function

    Private Sub AcceptMethod(Of T)(ByVal methodThatLooksLikeAny As Func(Of Func(Of T, Boolean), Boolean))
    End Sub

    Friend Class GoodList(Of T)
        Inherits List(Of T)
        Public Function GetList() As GoodList(Of T)
            Return Me
        End Function
    End Class

    Friend Class EnumList(Of T)
        Implements IEnumerable(Of T)
        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return Nothing
        End Function
        Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Nothing
        End Function
    End Class

    Friend Class ClassA
        Public myListField As List(Of Integer) = New List(Of Integer)()

        Public Property myListProperty As List(Of Integer)
            Get
                Return myListField
            End Get
            Set(ByVal value As List(Of Integer))
                myListField.AddRange(value)
                Dim b = myListField.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
                Dim c = myListField.Exists(Function(x) x > 0) ' Compliant
            End Set
        End Property

        Public classB As ClassB = New ClassB()
    End Class
End Class

Public Class ClassB
    Public myListField As List(Of Integer) = New List(Of Integer)()

    Public Function Any(ByVal predicate As Func(Of Integer, Boolean)) As Boolean
        Return False
    End Function

    Private Sub CheckEquals(ByVal intList As List(Of Integer), ByVal someInt As Integer)
        intList.Any(Function(x) Equals(x, someInt, someInt)) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
    End Sub

    Private Function Equals(ByVal a As Integer, ByVal b As Integer, ByVal c As Integer) As Boolean
        Return False
    End Function
End Class
