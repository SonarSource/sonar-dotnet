Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class UnitTest1

    <TestMethod()> Public Sub TestMethod1()
    End Sub

    <TestMethod()>
    <Ignore>
    Public Sub IgnoredWithTestAndAllScopeRuleIssues(B As Boolean)

        If B = True Then
           ' Do something.
        End If

        Assert.IsTrue(B)
    End Sub

End Class
