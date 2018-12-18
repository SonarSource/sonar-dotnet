Imports System
Imports NUnit.Framework

Namespace Tests.Diagnostics
    <TestFixture>
    Class ClassTest1
    End Class

    <TestFixture>
    Class ClassTest3
        Public Sub Foo()
        End Sub
    End Class

    Class ClassTest5
        Public Sub Foo()
        End Sub
    End Class

    Class ClassTest6
        Public Sub Foo()
        End Sub
    End Class

    <TestFixture>
    Class ClassTest7
        <Test>
        Public Sub Foo()
        End Sub
    End Class

    <TestFixture>
    Class ClassTest8
        <TestCase("")>
        <TestCase("test")>
        Public Sub Foo(ByVal a As String)
        End Sub
    End Class

    <TestFixture>
    Class ClassTest10
        <Theory>
        Public Sub Foo()
        End Sub
    End Class

    <TestFixture>
    Public MustInherit Class MyCommonCode2
    End Class

    <TestFixture>
    Public Class MySubCommonCode2
        Inherits MyCommonCode2
    End Class

    Public Class MySubCommonCode22
        Inherits MyCommonCode2
    End Class

    <TestFixture>
    Public Class ClassTest11
        <TestCaseSource("DivideCases")>
        Public Sub DivideTest(ByVal n As Integer, ByVal d As Integer, ByVal q As Integer)
            Assert.AreEqual(q, n / d)
        End Sub

        Shared DivideCases As Object() = {New Object() {12, 3, 4}, New Object() {12, 2, 6}, New Object() {12, 4, 3}}
    End Class

    <TestFixture>
    Public MustInherit Class TestFooBase
        <Test>
        Public Sub Foo_WhenFoo_ExpectsFoo()
        End Sub
    End Class

    <TestFixture>
    Public Class TestSubFoo
        Inherits TestFooBase
    End Class
End Namespace
