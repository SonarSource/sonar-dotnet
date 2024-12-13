Imports Xunit

Public Class Tests
    <Fact>
    Public Sub TestMethod1()
        Dim calculator As New Calculator()
        Dim result = calculator.Add(1, 2, Function(x) x > 0)
        Assert.Equal(3, result)
    End Sub
End Class

Public MustInherit Class BaseClass(Of T As Class)
    <Fact>
    Public Sub TestMethodInBaseClass()
        Assert.Equal(3, 3)
    End Sub

    <Fact>
    Public Overridable Sub VirtualMethodInBaseClass()
        Assert.Equal(1, 1)
    End Sub
End Class

Public NotInheritable Class Derived
    Inherits BaseClass(Of String)

    <Fact>
    Public Overrides Sub VirtualMethodInBaseClass()
        Assert.Equal(1, 1)
    End Sub
End Class

Public Class GenericTests
    <Theory>
    <InlineData(GetType(Integer))>
    <InlineData(GetType(String))>
    Public Sub GenericTestMethod(Of T)(type As Type)
        Console.WriteLine(type)
    End Sub
End Class

Public NotInheritable Class GenericDerivedFromGenericClass(Of T As Class, U As Class)
    Inherits BaseClass(Of T)

    <Fact>
    Public Sub GenericMethod(Of T, U)()
        Assert.Equal(1, 1)
    End Sub

End Class
