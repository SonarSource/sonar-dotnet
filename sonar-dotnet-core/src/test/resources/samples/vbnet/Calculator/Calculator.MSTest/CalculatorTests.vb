Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public NotInheritable Class CalculatorTests
    <TestMethod>
    Public Sub TestMethod1()
        Dim calculator As New Calculator()
        Dim result = calculator.Add(1, 2, Function(x) x > 0)
        Assert.AreEqual(3, result)
    End Sub

    <TestMethod>
    Public Sub GenericMethod(Of T As Class)()
        Assert.AreEqual(1, 1)
    End Sub
End Class

<TestClass>
Public MustInherit Class BaseClass(Of T As Class)
    <TestMethod>
    Public Sub TestMethodInBaseClass()
        Assert.AreEqual(1, 1)
    End Sub

    <TestMethod>
    Public Overridable Sub VirtualMethodInBaseClass()
        Assert.AreEqual(1, 1)
    End Sub
End Class

<TestClass>
Public NotInheritable Class Derived
    Inherits BaseClass(Of String)

    <TestMethod>
    Public Overrides Sub VirtualMethodInBaseClass()
        Assert.AreEqual(1, 1)
    End Sub
End Class

<TestClass>
Public NotInheritable Class GenericCalculatorTests(Of T As Class)
    Inherits BaseClass(Of T)

    <TestMethod>
    Public Sub Method()
        Assert.AreEqual(1, 1)
    End Sub

    <TestMethod>
    Public Sub GenericMethod(Of T)()
        Assert.AreEqual(1, 1)
    End Sub

    <TestMethod>
    Public Overrides Sub VirtualMethodInBaseClass()
        Assert.AreEqual(1, 1)
    End Sub
End Class

<TestClass>
Public Class GenericTests
    <DataTestMethod>
    <DataRow(GetType(Integer))>
    <DataRow(GetType(String))>
    Public Sub GenericTestMethod(Of T)(type As Type)
    End Sub
End Class
