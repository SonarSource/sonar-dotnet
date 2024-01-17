Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports NUnit.Framework
Imports System.Threading
Imports Xunit
Imports System.Threading.Thread
Imports _Alias = System.Threading.Thread

Class Compliant
    <Test>
    Public Sub TestWithoutThreadSleep()
        Xunit.Assert.Equal("42", "The answer to life, the universe, and everything")
    End Sub

    <Test>
    Public Sub CallToOtherSleepMethod()
        Sleep(42)
    End Sub

    Private Sub ThreadSleepNotInTest()
        Thread.Sleep(42)
    End Sub

    Private Sub Sleep(durartion As Integer)
    End Sub
End Class

Class NonCompliant
    <Test>
    Public Sub ThreadSleepInNUnitTest()
        Thread.Sleep(42) ' {{Do not use Thread.Sleep() in a test.}}
'       ^^^^^^^^^^^^^^^^
    End Sub

    <Fact>
    Public Sub ThreadSleepInXUnitTest()
        Thread.Sleep(42) ' Noncompliant
    End Sub

    <TestMethod>
    Public Sub ThreadSleepInMSTest()
        Thread.Sleep(42) ' Noncompliant
    End Sub

    <Test>
    Public Sub ThreadSleepViaAlias()
        _Alias.Sleep(42) ' Noncompliant
    End Sub
	
	<Test>
    Public Sub ThreadSleepViaStaticImport()
        Sleep(42) ' Noncompliant
    End Sub
	
	<Test>
    Public Sub ThreadSleepInLambdaFunction()
        Dim sleep As Action = Sub() Thread.Sleep(42) ' Noncompliant
    End Sub
End Class
