Imports System
Imports Xunit

Namespace Tests.Diagnostics
    Public Class XUnitTest
        <Xunit.Fact>
        Private Sub PrivateTestMethod()
        End Sub

        <Fact>
        Protected Sub ProtectedTestMethod()
        End Sub

        <Fact>
        Friend Sub InternalTestMethod()
        End Sub

        <Fact>
        Public Async Sub AsyncTestMethod()
        End Sub

        <Fact>
        Public Sub GenericTestMethod(Of T)()
        End Sub

        <Xunit.Theory>
        <InlineData(42)>
        Private Sub PrivateTestMethod_Theory(ByVal arg As Integer)
        End Sub

        <Theory>
        <InlineData(42)>
        Protected Sub ProtectedTestMethod_Theory(ByVal arg As Integer)
        End Sub

        <Theory>
        <InlineData(42)>
        Friend Sub InternalTestMethod_Theory(ByVal arg As Integer)
        End Sub

        <Theory>
        <InlineData(42)>
        Public Async Sub AsyncTestMethod_Theory(ByVal arg As Integer)
        End Sub

        <Theory>
        <InlineData(42)>
        Public Sub GenericTestMethod_Theory(Of T)(ByVal arg As T)
        End Sub
    End Class
End Namespace
