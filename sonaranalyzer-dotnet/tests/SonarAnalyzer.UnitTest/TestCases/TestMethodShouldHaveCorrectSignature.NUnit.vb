Imports NUnit.Framework

Namespace Tests.Diagnostics
    Class NUnitTest
        <NUnit.Framework.Test>
        Private Sub PrivateTestMethod()
        End Sub

        <Test>
        Protected Sub ProtectedTestMethod()
        End Sub

        <Test>
        Friend Sub InternalTestMethod()
        End Sub

        <Test>
        Public Async Sub AsyncTestMethod()
        End Sub

        <Test>
        Public Sub GenericTestMethod(Of T)()
        End Sub

        <Test>
        Public Sub ValidTest()
        End Sub
    End Class

    Class NUnitTest_TestCase
        <NUnit.Framework.TestCase(42)>
        Private Sub PrivateTestMethod(ByVal data As Integer)
        End Sub

        <TestCase(42)>
        Protected Sub ProtectedTestMethod(ByVal data As Integer)
        End Sub

        <TestCase(42)>
        Friend Sub InternalTestMethod(ByVal data As Integer)
        End Sub

        <TestCase(42)>
        Public Async Sub AsyncTestMethod(ByVal data As Integer)
        End Sub

        <TestCase(42)>
        Public Sub GenericTestMethod(Of T)(ByVal data As T)
        End Sub

        <TestCase(42)>
        Public Sub ValidTest(ByVal data As Integer)
        End Sub
    End Class

    Class NUnitTest_TestCaseSource
        Public Shared DataProvider As Object() = {42}

        <NUnit.Framework.TestCaseSource("DataProvider")>
        Private Sub PrivateTestMethod(ByVal data As Object)
        End Sub

        <TestCaseSource("DataProvider")>
        Protected Sub ProtectedTestMethod(ByVal data As Object)
        End Sub

        <TestCaseSource("DataProvider")>
        Friend Sub InternalTestMethod(ByVal data As Object)
        End Sub

        <TestCaseSource("DataProvider")>
        Public Async Sub AsyncTestMethod(ByVal data As Object)
        End Sub

        <TestCaseSource("DataProvider")>
        Public Sub GenericTestMethod(Of T)(ByVal data As T)
        End Sub

        <TestCaseSource("DataProvider")>
        Public Sub ValidTest(ByVal data As Object)
        End Sub
    End Class

    Public Class NUnitTest_Theories
        <NUnit.Framework.Theory>
        Private Sub PrivateTestMethod(ByVal data As Integer)
        End Sub

        <Theory>
        Protected Sub ProtectedTestMethod(ByVal data As Integer)
        End Sub

        <Theory>
        Friend Sub InternalTestMethod(ByVal data As Integer)
        End Sub

        <Theory>
        Public Async Sub AsyncTestMethod(ByVal data As Integer)
        End Sub

        <Theory>
        Public Sub GenericTestMethod(Of T)(ByVal data As T())
        End Sub

        <Theory>
        Public Sub ValidTest(ByVal data As Integer)
        End Sub
    End Class
End Namespace
