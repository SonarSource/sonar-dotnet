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
