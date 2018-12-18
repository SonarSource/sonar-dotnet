Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace Tests.Diagnostics
    <TestClass>
    Class ClassTest2
    End Class

    <TestClass>
    Class ClassTest4
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

    <TestClass>
    Class ClassTest9
        <TestMethod>
        Public Sub Foo()
        End Sub
    End Class

    <TestClass>
    Class ClassTest12
        <DataTestMethod>
        <DataRow(1)>
        Public Sub Foo(ByVal i As Integer)
        End Sub
    End Class

    <TestClass>
    Public MustInherit Class MyCommonCode1
    End Class

    <TestClass>
    Public Class MySubCommonCode1
        Inherits MyCommonCode1
    End Class

    Public Class MySubCommonCode11
        Inherits MyCommonCode1
    End Class

    <TestClass>
    Public MustInherit Class TestFooBase
        <TestMethod>
        Public Sub Foo_WhenFoo_ExpectsFoo()
        End Sub
    End Class

    <TestClass>
    Public Class Foo_WhenFoo_ExpectsFoo
        Inherits TestFooBase
    End Class
End Namespace

Namespace TestSetupAndCleanupAttributes
    <TestClass>
    Public Class SetupAttributes1
        <AssemblyInitialize>
        Public Shared Sub BeforeTests(ByVal context As TestContext)
        End Sub
    End Class

    <TestClass>
    Module SetupAttributes2
        <AssemblyCleanup>
        Sub AfterTests()
        End Sub
    End Module

    <TestClass>
    Module SetupAttributes3
        <AssemblyInitialize>
        Sub BeforeTests(ByVal context As TestContext)
        End Sub

        <AssemblyCleanup>
        Sub AfterTests()
        End Sub
    End Module
End Namespace
