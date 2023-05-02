Imports System
Imports System.Collections.Generic
Imports System.Linq

Public Class TestClass
    Private Function MyMethod(ByVal data As List(Of Integer)) As Boolean
        data.Any(Function(x) x > 0) ' Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
'       ^^^^^^^^
        data.Append(1).Any(Function(x) x > 0) ' Noncompliant
        data.Append(1).Append(2).Any(Function(x) x > 0) ' Noncompliant
        data.Append(1).Append(2).Any(Function(x) x > 0).ToString() ' Noncompliant

        data.Any() ' Compliant (you can't use Exists with no arguments, BC30455)
        data.Exists(Function(x) x > 0) ' Compliant

        Dim classA = New ClassA()
        classA.myListField.Any(Function(x) x > 0) ' Noncompliant
        classA.classB.myListField.Any(Function(x) x > 0) ' Noncompliant
        classA.classB.myListField.Any() ' Compliant

        Dim classB = New ClassB()
        classB.Any(Function(x) x) ' Compliant

        Dim boolList = New List(Of Boolean)()
        boolList.Append(boolList.Exists(Function(x) x)).Any(Function(x) x) ' Noncompliant

        data?.Any(Function(x) x > 0) ' FN
        data?.Any(Function(x) x > 0).ToString() ' FN
        classB?.Any(Function(x) x) ' Compliant

        Return data.Any(Function(x) x Mod 2 = 0) ' Noncompliant
    End Function

    Private Function ContainsEvenExpression(ByVal data As List(Of Integer)) As Boolean
        Return data.Any(Function(x) x Mod 2 = 0) ' Noncompliant
    End Function

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

        Public classB As ClassB = New ClassB()
    End Class
End Class

Public Class ClassB
    Public myListField As List(Of Integer) = New List(Of Integer)()

    Public Function Any(ByVal predicate As Func(Of Boolean, Boolean)) As Boolean
        Return False
    End Function
End Class
