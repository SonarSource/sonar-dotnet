Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Public Class CollectionTests

    Private Items As IEnumerable(Of Integer)
    Private Predicate As Predicate(Of Integer)
    Private Action As Action(Of Integer)

    Private Function GetList() As List(Of Integer)
    End Function

    Public Sub DefaultConstructor()
        Dim List As New List(Of Integer)
        List.Clear()                    ' Noncompliant {{Remove this call, the collection is known to be empty here.}}

        Dim HS As New HashSet(Of Integer)
        HS.Clear()                      ' Noncompliant
        Dim Q As New Queue(Of Integer)
        Q.Clear()                       ' Noncompliant
        Dim S As New Stack(Of Integer)
        S.Clear()                       ' Noncompliant
        Dim OC As New ObservableCollection(Of Integer)
        OC.Clear()                      ' Noncompliant
        Dim Arr(-1) As Integer
        Arr.Clone()                     ' Noncompliant
        Dim D As New Dictionary(Of Integer, Integer)
        D.Clear()                       ' Noncompliant
    End Sub

    Public Sub ConstructorWithCapacity()
        Const ConstCount0 As Integer = -1
        Const ConstCount5 As Integer = 5
        Dim VarCount0 As Integer = -1
        Dim VarCount5 As Integer = 5

        Dim List As New List(Of Integer)(5)
        List.Clear()                        ' Noncompliant
        Dim ArrLiteral5(5) As Integer
        ArrLiteral5.Clone()                 ' Compliant
        Dim ArrLiteral0(-1) As Integer
        ArrLiteral0.Clone()                 ' Noncompliant
        Dim ArrConst5(ConstCount5) As Integer
        ArrConst5.Clone()                   ' Compliant
        Dim ArrConst0(ConstCount0) As Integer
        ArrConst0.Clone()                   ' Noncompliant
        Dim ArrVar5(VarCount5) As Integer
        ArrConst5.Clone()                   ' Compliant
        Dim ArrVar0(VarCount0) As Integer
        ArrConst0.Clone()                   ' Noncompliant

        Dim ArrBinary(VarCount5 - ConstCount5 - 1) As Integer
        ArrBinary.Clone()                   ' FN
    End Sub

    Public Sub ConstructorWithEnumerable()
        Dim List As New List(Of Integer)(Items)
        List.Clear()                    ' Compliant
    End Sub

    Public Sub ConstructorWithEnumerableWithConstraint(Condition As Boolean)
        Dim BaseCollection As New List(Of Integer)
        Dim HS As New HashSet(Of Integer)(BaseCollection)
        HS.Clear()                      ' Noncompliant

        BaseCollection = New List(Of Integer)
        HS = New HashSet(Of Integer)(BaseCollection, EqualityComparer(Of Integer).Default)
        HS.Clear()                      ' Noncompliant

        BaseCollection = New List(Of Integer)
        HS = New HashSet(Of Integer)(comparer:=EqualityComparer(Of Integer).Default, collection:=BaseCollection)
        HS.Clear()                      ' Noncompliant

        BaseCollection = New List(Of Integer)
        HS = New HashSet(Of Integer)(If(Condition, BaseCollection, BaseCollection))
        HS.Clear()                      ' Noncompliant

        BaseCollection.Add(1)
        HS = New HashSet(Of Integer)(BaseCollection)
        HS.Clear()                      ' Compliant
    End Sub

    Public Sub ConstructorWithEmptyInitializer()
        Dim List As New List(Of Integer) From {}
        List.Clear()                    ' Noncompliant
    End Sub

    Public Sub ConstructorWithInitializer()
        Dim List As New List(Of Integer) From {1, 2, 3}
        List.Clear()                    ' Compliant
        Dim Array() As Integer = {1, 2, 3}
        Array.Clone()                   ' Compliant
    End Sub

    Public Sub Other_Initialization()
        Dim List As List(Of Integer) = GetList()
        List.Clear()                    ' Compliant
        Dim Array As Integer() = Array.Empty(Of Integer)
        Array.Clone()                   ' FN
        Dim Enm As IEnumerable(Of Integer) = Enumerable.Empty(Of Integer)
        Enm.GetEnumerator()             ' FN
    End Sub

    Public Sub Methods_Raise_Issue()
        Dim List As New List(Of Integer), i As Integer
        List.BinarySearch(5)            ' Noncompliant
        List.Clear()                    ' Noncompliant
        List.Contains(5)                ' Noncompliant
        List.ConvertAll(Function(X) X)  ' Noncompliant
        List.CopyTo(Nothing, 1)         ' Noncompliant
        List.Exists(Predicate)          ' Noncompliant
        List.Find(Predicate)            ' Noncompliant
        List.FindAll(Predicate)         ' Noncompliant
        List.FindIndex(Predicate)       ' Noncompliant
        List.FindLast(Predicate)        ' Noncompliant
        List.FindLastIndex(Predicate)   ' Noncompliant
        List.ForEach(Action)            ' Noncompliant
        List.GetEnumerator()            ' Noncompliant
        List.GetRange(1, 5)             ' Noncompliant
        List.IndexOf(5)                 ' Noncompliant
        List.LastIndexOf(5)             ' Noncompliant
        List.Remove(5)                  ' Noncompliant
        List.RemoveAll(Predicate)       ' Noncompliant
        List.RemoveAt(1)                ' Noncompliant
        List.RemoveRange(1, 5)          ' Noncompliant
        List.Reverse()                  ' Noncompliant
        List.Sort()                     ' Noncompliant
        List.TrueForAll(Predicate)      ' Noncompliant
        i = List(1)                     ' Compliant, should be part of S6466
        List(1) = 5                     ' Compliant, should be part of S6466
        Dim ToArr() As Integer = List.ToArray
        ToArr.Clone()                   ' FN

        Dim Array(-1) As Integer
        Array.Clone()                   ' Noncompliant
        Array.CopyTo(Nothing, 1)        ' Noncompliant
        Array.GetEnumerator()           ' Noncompliant
        Array.GetLength(1)              ' Noncompliant
        Array.GetLongLength(1)          ' Noncompliant
        Array.GetLowerBound(1)          ' Noncompliant
        Array.GetUpperBound(1)          ' Noncompliant
        Array.GetValue(1)               ' Noncompliant
        Array.Initialize()              ' Noncompliant
        Array.SetValue(5, 1)            ' Noncompliant
        i = Array(1)                    ' Compliant, should be part of S6466
        Array(1) = 5                    ' Compliant, should be part of S6466
    End Sub

    Public Sub Methods_Ignored()
        Dim List As New List(Of Integer)
        List.AsReadOnly()
        List.GetHashCode()
        List.GetType()
        List.Equals(Items)
        List.ToString()
        List.TrimExcess()
        List.ToArray()

        Dim Array(5) As Integer
        Array.GetHashCode()
        Array.Equals(New Object())
        Array.GetType()
        Array.ToString()
        Dim Length As Integer = Array.Length
    End Sub

    Public Sub Methods_Set_NotEmpty()
        Dim List As New List(Of Integer)
        List.Add(5)
        List.Clear()                   ' Compliant
        List = New List(Of Integer)
        List.AddRange(Items)
        List.Clear()                   ' Compliant
        List = New List(Of Integer)
        List.Insert(1, 5)
        List.Clear()                   ' Compliant
        List = New List(Of Integer)
        List.InsertRange(1, Items)
        List.Clear()                   ' Compliant
    End Sub

    Public Sub Method_Set_Empty(List As List(Of Integer))
        List.Clear()                    ' Compliant
        List.Clear()                    ' Noncompliant

        Dim Empty As New List(Of Integer)
        List.Add(5)
        List.Intersect(Empty)           ' Compliant
        List.Clear()                    ' FN
    End Sub

    Public Sub ForEach()
        Dim List As New List(Of Integer)
        For Each i As Integer In List   ' Noncompliant
        Next
    End Sub

    Public Sub Casing()
        Dim List As New List(Of Integer)
        List.Clear()                    ' Noncompliant
    End Sub

    Public Sub LearnConditions_Size(Condition As Boolean)
        Dim IsNull As List(Of Integer) = Nothing
        Dim Empty As New List(Of Integer)
        Dim NotEmpty As New List(Of Integer) From {1, 2, 3}

        If If(IsNull, Empty).Count = 0 Then Empty.Clear()  ' Noncompliant
        If If(IsNull, NotEmpty).Count = 0 Then Empty.Clear()  ' Compliant, unreachable
        If If(Condition, Empty, Empty).Count = 0 Then
            Empty.Clear()  ' Noncompliant
        Else
            Empty.Clear()  ' Compliant, unreachable
        End If
        If Empty.Count = 0 Then
            Empty.Clear()  ' Noncompliant
        Else
            Empty.Clear()  ' Compliant, unreachable
        End If
        If Empty.Count() = 0 Then
            Empty.Clear()  ' Noncompliant
        Else
            Empty.Clear()  ' Compliant, unreachable
        End If
        If DirectCast(Empty, IEnumerable(Of Integer)).Count(Function(X) Condition) = 0 Then
            Empty.Clear()   ' Noncompliant
        Else
            Empty.Clear()   ' Compliant, unreachable
        End If
        If DirectCast(NotEmpty, IEnumerable(Of Integer)).Count(Function(X) Condition) = 0 Then
            Empty.Clear()   ' Noncompliant
        Else
            Empty.Clear()   ' Noncompliant
        End If
        If Enumerable.Count(Empty) = 0 Then
            Empty.Clear()  ' Noncompliant
        Else
            Empty.Clear()  ' Compliant, unreachable
        End If
        NotEmpty.Clear()   ' Compliant, prevents LVA from throwing notEmpty away during reference capture
    End Sub

    Public Sub Enumerable_ExtensionIsInstance()
        Dim Empty As New List(Of Integer)
        Dim AsIEnumerable As IEnumerable(Of Integer) = Empty
        If AsIEnumerable.Count = 0 Then ' This is invoked as instance, not as a static call
            Empty.Clear()   ' Noncompliant
        Else
            Empty.Clear()   ' Compliant, unreachable
        End If
    End Sub

End Class

Public Class AdvancedTests

    Public Sub UnknownExtensionMethods()
        Dim List As New List(Of Integer)
        List.CustomExtensionMethod()    ' Compliant
        List.Clear()                    ' Noncompliant FP
    End Sub

    Public Sub WellKnownExtensionMethods()
        Dim List As New List(Of Integer)
        List.All(Function(X) True)      ' FN
        List.Any()                      ' FN
        Enumerable.Reverse(List)        ' Noncompliant
        List.Clear()                    ' Noncompliant
    End Sub

    Public Sub PassingAsArgument_Removes_Constraints()
        Dim List As New List(Of Integer)
        Foo(List)
        List.Clear()   ' Compliant
    End Sub

    Public Sub HigherRank_And_Jagged_Array()
        Dim Array1(-1, -1) As Integer
        Array1.Clone()                  ' Noncompliant
        Dim Array2(-1, 4) As Integer
        Array2.Clone()                  ' Noncompliant
        Dim Array3(5, 4) As Integer
        Array3.Clone()                  ' Compliant
        Dim Array4()() As Integer = New Integer(-1)() {}
        Array4.Clone()                  ' Noncompliant
        Dim Array5()() As Integer = New Integer(1)() {}
        Array5.Clone()                  ' Compliant
    End Sub

    Private Sub Foo(Items As IEnumerable(Of Integer))
    End Sub

End Class

Public Module CustomExtensions

    <Runtime.CompilerServices.Extension>
    Public Sub CustomExtensionMethod(List As List(Of Integer))
    End Sub

End Module

' This simulates the Dictionary from .NetCore 2.0+.
Public Class NetCoreDictionary(Of TKey, TValue)
    Inherits Dictionary(Of TKey, TValue)

    Public Sub TestTryAdd()
        Dim Dict As New NetCoreDictionary(Of String, Object)
        If Dict.TryAdd("foo", New Object()) Then   ' Compliant
        End If
    End Sub

    Public Function TryAdd(Key As TKey, Value As TValue) As Boolean
    End Function

End Class

Public Class Flows

    Public Sub Conditional_Add(Condition As Boolean)
        Dim List As New List(Of Integer)
        If Condition Then List.Add(5)
        List.Clear()   ' Compliant
    End Sub

    Public Sub Conditional_Add_With_Loop(Condition As Boolean)
        Dim List As New List(Of Integer)
        While True
            If Condition Then List.Add(5)
            Exit While
        End While
        List.Clear()   ' Compliant
    End Sub

    ' https://github.com/SonarSource/sonar-dotnet/issues/4261
    Public Sub AddPassedAsParameter()
        Dim List As New List(Of Integer)
        DoSomething(AddressOf List.Add)
        List.Clear()   ' Compliant
    End Sub

    Public Sub CountZero(List As List(Of Integer))
        If List.Count = 0 Then
            List.Clear()        ' Noncompliant
        Else
            List.Clear()        ' Compliant
        End If
    End Sub

    Public Sub CountZeroExtension(List As List(Of Integer))
        If Enumerable.Count(List) = 0 Then
            List.Clear()        ' FN
        Else
            List.Clear()        ' Compliant
        End If
    End Sub

    Public Sub CountNotZero(List As List(Of Integer))
        If List.Count <> 0 Then
            List.Clear()        ' Compliant
        Else
            List.Clear()        ' Noncompliant
        End If
    End Sub

    Public Sub CountGreaterThanZero(List As List(Of Integer))
        If List.Count > 0 Then
            List.Clear()        ' Compliant
        Else
            List.Clear()        ' Noncompliant
        End If
    End Sub

    Public Sub CountGreaterThanOne(List As List(Of Integer))
        If List.Count > 1 Then
            List.Clear()        ' Compliant
        Else
            List.Clear()        ' Compliant
        End If
    End Sub

    Private Shared Sub DoSomething(Callback As Action(Of Integer))
    End Sub

End Class
