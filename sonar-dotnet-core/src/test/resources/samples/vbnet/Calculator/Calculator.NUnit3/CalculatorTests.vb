Imports NUnit.Framework

<TestFixture>
Public Class Tests
    <Test>
    Public Sub TestMethod1()
        Dim calculator As New Calculator()
        Dim result = calculator.Add(1, 2, Function(x) x > 0)
        Assert.That(result, [Is].EqualTo(3))
    End Sub
End Class

<TestFixture>
Public Class BaseClass(Of T As Class)
    <Test>
    Public Sub TestMethodInBaseClass()
        Assert.AreEqual(1, 1)
    End Sub

    <Test>
    Public Overridable Sub VirtualMethodInBaseClass()
        Assert.AreEqual(1, 1)
    End Sub
End Class

<TestFixture>
Public NotInheritable Class Derived
    Inherits BaseClass(Of String)

    <Test>
    Public Overrides Sub VirtualMethodInBaseClass()
        Assert.AreEqual(1, 1)
    End Sub
End Class

<TestFixture>
Public NotInheritable Class GenericCalculatorTests(Of T As Class)
    Inherits BaseClass(Of T)

    <Test>
    Public Sub Method()
        Assert.AreEqual(1, 1)
    End Sub

    <Test>
    Public Sub GenericMethod(Of T)()
        Assert.AreEqual(1, 1)
    End Sub

    <Test>
    Public Overrides Sub VirtualMethodInBaseClass()
        Assert.AreEqual(1, 1)
    End Sub
End Class

<TestFixture>
Public Class GenericTests
    <TestCase(42)>
    <TestCase("string")>
    <TestCase(Double.Epsilon)>
    Public Sub GenericTest(Of T)(instance As T)
        Console.WriteLine(instance)
    End Sub
End Class
