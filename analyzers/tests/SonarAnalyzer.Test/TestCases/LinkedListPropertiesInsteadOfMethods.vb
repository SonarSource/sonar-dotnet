Imports System.Collections
Imports System
Imports System.Collections.Generic
Imports System.Linq

Class [MyClass]
    Private Sub Various()
        Dim data = New LinkedList(Of Integer)()
        Enumerable.First(data) ' Noncompliant {{'First' property of 'LinkedList' should be used instead of the 'First()' extension method.}}
        '          ^^^^^
        Enumerable.Last(data)  ' Noncompliant {{'Last' property of 'LinkedList' should be used instead of the 'Last()' extension method.}}
        '          ^^^^

        Dim a = Enumerable.Any(data)    ' Compliant
        Dim b1 = data.First()           ' Compliant
        Dim b2 = data.Last()            ' Compliant
        Dim c1 = data.First.Value       ' Compliant
        Dim c2 = data.Last.Value        ' Compliant
        Dim d = data.Count()            ' Compliant
        Dim e1 = Enumerable.First(data, Function(x) x > 0) ' Compliant
        Dim e2 = Enumerable.Last(data, Function(x) x > 0)  ' Compliant

        Enumerable.First(If(True = True, data, data)) ' Noncompliant
    End Sub
End Class
