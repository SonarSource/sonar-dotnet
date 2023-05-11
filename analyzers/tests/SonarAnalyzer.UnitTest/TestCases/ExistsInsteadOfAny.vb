Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq

Public Class TestClass
    Private Sub MyMethod(ByVal list As List(Of Integer), ByVal array As Integer())
        list.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        '    ^^^

        list.Append(1).Any(Function(x) x > 1) ' Compliant (Appended list becomes an IEnumerable)
        list.Append(1).Append(2).Any(Function(x) x > 1) ' Compliant
        Enumerable.Any(Enumerable.Append(Enumerable.Append(list, 1), 2), Function(x) x > 1).ToString() ' Compliant 

        list.Any() ' Compliant (you can't use Exists with no arguments, CS7036)
        list.Exists(Function(x) x > 0) ' Compliant

        array.Any(Function(x) x > 0) ' Noncompliant
        array.Any() ' Compliant

        Dim classA = New ClassA()
        classA.myListField.Any(Function(x) x > 0) ' Noncompliant
        classA.classB.myListField.Any(Function(x) x > 0) ' Noncompliant
        classA.classB.myListField.Any() ' Compliant

        Dim classB = New ClassB()
        classB.Any(Function(x) x > 0) ' Compliant

        list?.Any(Function(x) x > 0) ' Noncompliant
        classB?.Any(Function(x) x > 0) ' Compliant

        Dim del As Func(Of Integer, Boolean) = Function(x) True
        list.Any(del) ' Noncompliant

        Dim enumList = New EnumList(Of Integer)()
        enumList.Any(Function(x) x > 0) ' Compliant

        Dim goodList = New GoodList(Of Integer)()
        goodList.Any(Function(x) x > 0) ' Noncompliant

        Dim ternary = If(True, list, goodList).Any(Function(x) x > 0) ' Noncompliant
        Dim nullCoalesce = If(list, goodList).Any(Function(x) x > 0) ' Noncompliant
        Dim ternaryNullCoalesce = If(list, If(True, list, goodList)).Any(Function(x) x > 0) ' Noncompliant

        goodList.GetList().Any(Function(x) True) ' Noncompliant

        Any(Of Integer)(Function(x) x > 0) ' Compliant
        Call AcceptMethod(New Func(Of Func(Of Integer, Boolean), Boolean)(AddressOf goodList.Any)) ' Compliant
    End Sub

    Private Sub ConditionalsMatrix(ByVal goodList As GoodList(Of Integer))
        goodList.GetList().GetList().GetList().GetList().Any(Function(x) x > 0)     ' Noncompliant
        goodList.GetList().GetList().GetList().GetList()?.Any(Function(x) x > 0)    ' Noncompliant
        goodList.GetList().GetList().GetList()?.GetList().Any(Function(x) x > 0)    ' Noncompliant
        goodList.GetList().GetList().GetList()?.GetList()?.Any(Function(x) x > 0)   ' Noncompliant
        goodList.GetList().GetList()?.GetList().GetList().Any(Function(x) x > 0)    ' Noncompliant
        goodList.GetList().GetList()?.GetList().GetList()?.Any(Function(x) x > 0)   ' Noncompliant
        goodList.GetList().GetList()?.GetList()?.GetList().Any(Function(x) x > 0)   ' Noncompliant
        goodList.GetList().GetList()?.GetList()?.GetList()?.Any(Function(x) x > 0)  ' Noncompliant
        goodList.GetList()?.GetList().GetList().GetList().Any(Function(x) x > 0)    ' Noncompliant
        goodList.GetList()?.GetList().GetList().GetList()?.Any(Function(x) x > 0)   ' Noncompliant
        goodList.GetList()?.GetList().GetList()?.GetList().Any(Function(x) x > 0)   ' Noncompliant
        goodList.GetList()?.GetList().GetList()?.GetList()?.Any(Function(x) x > 0)  ' Noncompliant
        goodList.GetList()?.GetList()?.GetList().GetList().Any(Function(x) x > 0)   ' Noncompliant
        goodList.GetList()?.GetList()?.GetList().GetList()?.Any(Function(x) x > 0)  ' Noncompliant
        goodList.GetList()?.GetList()?.GetList()?.GetList().Any(Function(x) x > 0)  ' Noncompliant
        goodList.GetList()?.GetList()?.GetList()?.GetList()?.Any(Function(x) x > 0) ' Noncompliant
    End Sub

    Private Sub CheckDelegate(ByVal intList As List(Of Integer), ByVal stringList As List(Of String), ByVal refList As List(Of ClassA), ByVal intArray As Integer(), ByVal someString As String, ByVal someInt As Integer, ByVal anotherInt As Integer, ByVal someRef As ClassA)
        intList.Any(Function(x) x = 0) ' Compliant (should raise S6617)
        intList.Any(Function(x) 0 = x) ' Compliant (should raise S6617)
        intList.Any(Function(x) x = someInt) ' Compliant (should raise S6617)
        intList.Any(Function(x) someInt = x) ' Compliant (should raise S6617)
        intList.Any(Function(x) x.Equals(0)) ' Compliant (should raise S6617)
        intList.Any(Function(x) 0.Equals(x)) ' Compliant (should raise S6617)

        intList.Any(Function(x) x = x) ' Noncompliant
        intList.Any(Function(x) someInt = anotherInt) ' Noncompliant
        intList.Any(Function(x) someInt = 0) ' Noncompliant
        intList.Any(Function(x) 0 = 0) ' Noncompliant

        intList.Any(Function(x) x.Equals(x)) ' Noncompliant
        intList.Any(Function(x) someInt.Equals(anotherInt)) ' Noncompliant
        intList.Any(Function(x) someInt.Equals(0)) ' Noncompliant
        intList.Any(Function(x) 0.Equals(0)) ' Noncompliant
        intList.Any(Function(x) x.Equals(x + 1)) ' Noncompliant

        intList.Any(Function(x) x.GetType() Is GetType(Integer)) ' Noncompliant
        intList.Any(Function(x) x.GetType().Equals(GetType(Integer))) ' Noncompliant FP
        intList.Any(Function(x) MyIntCheck(x)) ' Noncompliant
        intList.Any(Function(x) x <> 0)     ' Noncompliant
        intList.Any(Function(x) x.Equals(0) AndAlso True)   ' Noncompliant
        intList.Any(Function(x) If(x = 0, 2, 0) = 0) ' Noncompliant

        stringList.Any(Function(x) Equals(x, "")) ' Compliant (should raise S6617)
        stringList.Any(Function(x) Equals("", x)) ' Compliant (should raise S6617)
        stringList.Any(Function(x) Equals(x, someString)) ' Compliant (should raise S6617)
        stringList.Any(Function(x) Equals(someString, x)) ' Compliant (should raise S6617)
        stringList.Any(Function(x) x.Equals("")) ' Compliant (should raise S6617)
        stringList.Any(Function(x) "".Equals(x)) ' Compliant (should raise S6617)
        stringList.Any(Function(x) Equals(x, "")) ' Compliant (should raise S6617)

        stringList.Any(Function(x) MyStringCheck(x)) ' Noncompliant
        stringList.Any(Function(x) Not Equals(x, ""))     ' Noncompliant
        stringList.Any(Function(x) x.Equals("") AndAlso True)   ' Noncompliant
        stringList.Any(Function(x) Equals(If(Equals(x, ""), "a", "b"), "a")) ' Noncompliant
        stringList.Any(Function(x) x.Equals("" & someString)) ' Noncompliant

        intArray.Any(Function(x) x = 0) ' Noncompliant (this is not raising S6617)
        intArray.Any(Function(x) 0 = x) ' Noncompliant (this is not raising S6617)
        intArray.Any(Function(x) x = someInt) ' Noncompliant (this is not raising S6617)
        intArray.Any(Function(x) someInt = x) ' Noncompliant (this is not raising S6617)
        intArray.Any(Function(x) x.Equals(0)) ' Noncompliant (this is not raising S6617)
        intArray.Any(Function(x) 0.Equals(x)) ' Noncompliant (this is not raising S6617)
        intArray.Any(Function(x) someInt.Equals(x)) ' Noncompliant (this is not raising S6617)
        intArray.Any(Function(x) x.Equals(x + 1)) ' Noncompliant (this is not raising S6617)

        refList.Any(Function(x) x Is someRef) ' Noncompliant (this is not raising S6617)
        refList.Any(Function(x) someRef Is x) ' Noncompliant (this is not raising S6617)
        refList.Any(Function(x) x.Equals(someRef)) ' Compliant (should raise S6617)
        refList.Any(Function(x) someRef.Equals(x)) ' Compliant (should raise S6617)
        refList.Any(Function(x) Equals(someRef, x)) ' Compliant (should raise S6617)
        refList.Any(Function(x) Equals(x, someRef)) ' Compliant (should raise S6617)

        intList.Any(Function(x) x Is Nothing) ' Error [BC30020]
        intList.Any(Function(x) x.Equals(Nothing)) ' Compliant FN (warning: the result of this expression will always be false since a value-type is never equal to null)
        intList.Any(Function(x) Equals(x, Nothing)) ' Compliant FN (warning: the result of this expression will always be false since a value-type is never equal to null)

        refList.Any(Function(x) x = Nothing) ' Error [BC30452]
        refList.Any(Function(x) x.Equals(Nothing)) ' Compliant (should raise S6617)
        refList.Any(Function(x) Equals(x, Nothing)) ' Compliant (should raise S6617)
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
                Dim b = myListField.Any(Function(x) x > 0) ' Noncompliant
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
End Class
