Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace Tests.Diagnostics
    <TestClass>
    Public Class MsTestTest
        <Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod>
        Private Sub PrivateTestMethod()
        End Sub

        <TestMethod>
        Protected Sub ProtectedTestMethod()
        End Sub

        <TestMethod>
        Friend Sub InternalTestMethod()
        End Sub

        <TestMethod>
        Public Async Sub AsyncTestMethod()
        End Sub

        <TestMethod>
        Public Sub GenericTestMethod(Of T)()
        End Sub

        <TestMethod>
        Private Sub MultiErrorsMethod1(Of T)()
        End Sub

        <TestMethod>
        Private Async Sub MultiErrorsMethod2(Of T)()
        End Sub

        <TestMethod>
        Public Async Function DoSomethingAsync() As Task
            Return
        End Function
    End Class
End Namespace

Namespace Tests.Diagnostics
    <TestClass>
    Public Class MsTestTest_DataTestMethods
        <Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethod>
        Private Sub PrivateTestMethod()
        End Sub

        <DataTestMethod>
        Protected Sub ProtectedTestMethod()
        End Sub

        <DataTestMethod>
        Friend Sub InternalTestMethod()
        End Sub

        <DataTestMethod>
        Public Async Sub AsyncTestMethod()
        End Sub

        <DataTestMethod>
        Public Sub GenericTestMethod(Of T)()
        End Sub

        <DataTestMethod>
        Private Sub MultiErrorsMethod1(Of T)()
        End Sub

        <DataTestMethod>
        Private Async Sub MultiErrorsMethod2(Of T)()
        End Sub

        <DataTestMethod>
        Public Async Function DoSomethingAsync() As Task
            Return
        End Function
    End Class
End Namespace
