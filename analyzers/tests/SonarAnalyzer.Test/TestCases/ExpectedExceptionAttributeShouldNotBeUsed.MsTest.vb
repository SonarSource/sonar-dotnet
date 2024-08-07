Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public Class ExceptionTests

    ' Noncompliant@+2
    <TestMethod>
    <ExpectedException(GetType(DivideByZeroException))>
    Public Sub ExpectedExceptionAttrbutesShouldNotBeUsedOnSub()
        Dim instance As Integer = 42
        instance.ToString()
    End Sub

    ' Noncompliant@+2
    <TestMethod>
    <ExpectedException(GetType(DivideByZeroException))>
    Public Function ExpectedExceptionAttrbutesShouldNotBeUsedOnFunction() As String
        Dim instance As Integer = 42
        Return instance.ToString()
    End Function

    ' Compliant - one line
    <TestMethod>
    <ExpectedException(GetType(DivideByZeroException))>
    Public Sub ExpectedExceptionAttrbuteAllowedForSingleLineSub()
        Throw New NotImplementedException()
    End Sub

    ' Compliant - one line
    <TestMethod>
    <ExpectedException(GetType(DivideByZeroException))>
    Public Function ExpectedExceptionAttrbuteAllowedForSingleLineFunction() As Integer
        Return 42
    End Function
End Class

' https://github.com/SonarSource/sonar-dotnet/issues/8300
Class Repro_8300
    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))>
    Public Sub AssertInFinally()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Finally
            Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor)
        End Try
    End Sub

    ' Noncompliant@+2
    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))>
    Public Sub NoAssertInFinally()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Finally
            Console.WriteLine("No Assert")
        End Try
    End Sub

    ' Noncompliant@+2
    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))>
    Public Sub NoAssertInCatch()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Catch e As InvalidOperationException
            Console.ForegroundColor = ConsoleColor.Black
        End Try
    End Sub

    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))>
    Public Sub AssertInCatch()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Catch e As InvalidOperationException
            Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor)
        End Try
    End Sub

    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))>
    Public Sub AssertInAllCatch()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Catch e As InvalidOperationException
            Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor)
        End Try
    End Sub
End Class

