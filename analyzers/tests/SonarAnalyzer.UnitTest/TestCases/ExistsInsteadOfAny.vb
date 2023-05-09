Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq

Public Class TestClass
    Private Function MyMethod(ByVal data As List(Of Integer)) As Boolean
        data.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        '    ^^^
        data.Append(1).Any(Function(x) x > 0) ' Compliant
        data.Append(1).Append(2).Any(Function(x) x > 0) ' Compliant
        data.Append(1).Append(2).Any(Function(x) x > 0).ToString() ' Compliant

        data.Any() ' Compliant (you can't use Exists with no arguments, BC30455)
        data.Exists(Function(x) x > 0) ' Compliant

        Dim classA = New ClassA()
        classA.myListField.Any(Function(x) x > 0) ' Noncompliant
        classA.classB.myListField.Any(Function(x) x > 0) ' Noncompliant
        classA.classB.myListField.Any() ' Compliant

        Dim classB = New ClassB()
        classB.Any(Function(x) x > 0) ' Compliant

        data?.Any(Function(x) x > 0) ' Noncompliant
        data?.Any(Function(x) x > 0).ToString() ' Noncompliant
        classB?.Any(Function(x) x > 0) ' Compliant

        Dim goodList = New GoodList(Of Integer)()

        goodList.GetList().Any(Function(x) x > 0) ' Noncompliant

        Any(Of Integer)(Function(x) x > 0) ' Compliant

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

        Return data.Any(Function(x) x Mod 2 = 0) ' Noncompliant
    End Function

    Private Sub CheckDelegate(ByVal intList As List(Of Integer), ByVal stringList As List(Of String), ByVal customList As List(Of ClassA), ByVal someString As String)
        intList.Any(Function(x) x = 0) ' Compliant (should raise S6617)
        intList.Any(Function(x) 0 = x) ' Compliant (should raise S6617)
        intList.Any(Function(x) x.Equals(0)) ' Compliant (should raise S6617)
        intList.Any(Function(x) 0.Equals(x)) ' Compliant (should raise S6617)
        intList.Any(Function(x) x.Equals(x + 1)) ' Compliant (should raise S6617)
        intList.Any(Function(x) x.Equals(0) AndAlso True) ' Noncompliant
        intList.Any(Function(x) (If(x = 0, 2, 0)) = 0) ' Noncompliant

        stringList.Any(Function(x) x = "") ' Compliant (should raise S6617)
        stringList.Any(Function(x) "" = x) ' Compliant (should raise S6617)
        stringList.Any(Function(x) x.Equals("")) ' Compliant (should raise S6617)
        stringList.Any(Function(x) "".Equals(x)) ' Compliant (should raise S6617)
        stringList.Any(Function(x) x.Equals("" & someString)) ' Compliant (should raise S6617)
        stringList.Any(Function(x) x.Equals("") AndAlso True) ' Noncompliant
        stringList.Any(Function(x) (If(x = "", "a", "b")) = "a") ' Noncompliant

        customList.Any(Function(x) x.Equals()) ' FN
    End Sub

    Private Function ContainsEvenExpression(ByVal data As List(Of Integer)) As Boolean
        Return data.Any(Function(x) x Mod 2 = 0) ' Noncompliant
    End Function

    Private Function Any(Of T)(ByVal predicate As Func(Of T, Boolean)) As Boolean
        Return True
    End Function

    Private Sub AcceptMethod(Of T)(ByVal methodThatLooksLikeAny As Func(Of Func(Of T, Boolean), Boolean))
    End Sub

    Private Class GoodList(Of T)
        Inherits List(Of T)

        Public Function GetList() As GoodList(Of T)
            Return Me
        End Function

        Private Sub CallAny()
            Me.Any(Function(x) True) ' Noncompliant
        End Sub
    End Class

    Private Class EnumList(Of T)
        Implements IEnumerable(Of T)

        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return Nothing
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Nothing
        End Function
    End Class

    Public Class ClassA
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

        Public Function Equals() As Boolean
            Return False
        End Function

        Public classB As ClassB = New ClassB()
    End Class
End Class

Public Class ClassB
    Public myListField As List(Of Integer) = New List(Of Integer)()
    Public Function Any(ByVal predicate As Func(Of Boolean, Boolean)) As Boolean
        Return False
    End Function
End Class
