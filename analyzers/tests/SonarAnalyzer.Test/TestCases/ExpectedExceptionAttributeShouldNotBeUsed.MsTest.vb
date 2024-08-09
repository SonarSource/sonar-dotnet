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

    <TestMethod>
    <ExpectedExceptionAttribute(GetType(ArgumentNullException))>  ' Noncompliant
    Public Sub WithAttributeSuffix()
        Dim x As Boolean = True
        x.ToString()
    End Sub

    <TestMethod>
    <Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedException(GetType(ArgumentNullException))>  ' Noncompliant
    Public Sub FullyQualifiedAttribute()
        Dim x As Boolean = True
        x.ToString()
    End Sub

    <TestMethod>
    <Unrelated.ExpectedException(GetType(ArgumentNullException))>  ' Noncompliant - FPgit
    Public Sub UnrelatedAttribute()
        Dim x As Boolean = True
        x.ToString()
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
    <ExpectedException(GetType(InvalidOperationException))> ' Compliant - using ExpectedException makes the test more readable
    Public Sub AssertInFinally()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Finally
            Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor)
        End Try
    End Sub

    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))> ' Noncompliant
    Public Sub NoAssertInFinally()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Finally
            Console.WriteLine("No Assert")
        End Try
    End Sub

    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))> ' Noncompliant
    Public Sub NoAssertInCatch()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Catch e As InvalidOperationException
            Console.ForegroundColor = ConsoleColor.Black
        End Try
    End Sub

    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))> ' Compliant
    Public Sub AssertInCatch()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Catch e As InvalidOperationException
            Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor)
        End Try
    End Sub

    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))> ' Compliant
    Public Sub AssertInAllCatch()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Catch
            Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor)
        End Try
    End Sub

    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))> ' Compliant
    Public Sub AssertInAllCatch_InvocationBeforeAssert()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Catch
            Console.WriteLine("An invocation before Assert")
            Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor)
        End Try
    End Sub

    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))> ' Compliant
    Public Sub AssertInAllCatch_InvocationAfterAssert()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Catch
            Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor)
            Console.WriteLine("An invocation after Assert")
        End Try
    End Sub

    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))> ' Compliant
    Public Sub AssertInFinallyWithCatch()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Catch e As InvalidOperationException
            Console.WriteLine(Console.ForegroundColor)
        Finally
            Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor)
        End Try
    End Sub

    <TestMethod>
    <ExpectedException(GetType(InvalidOperationException))> ' Compliant
    Public Sub AssertInCatchWithFinally()
        Console.ForegroundColor = ConsoleColor.Red
        Try
            Throw New InvalidOperationException()
        Catch e As InvalidOperationException
            Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor)
        Finally
            Console.WriteLine(Console.ForegroundColor)
        End Try
    End Sub
End Class

Namespace Unrelated
    <AttributeUsage(AttributeTargets.Method)>
    Public Class ExpectedExceptionAttribute
        Inherits Attribute

        Public ReadOnly Property ExceptionType As Type

        Public Sub New(exceptionType As Type)
            Me.ExceptionType = exceptionType
        End Sub
    End Class
End Namespace
