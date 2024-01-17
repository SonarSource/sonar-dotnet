Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public Class ExceptionTests

    <TestMethod>
    <ExpectedException(GetType(DivideByZeroException))> ' Noncompliant
    Public Sub ExpectedExceptionAttrbutesShouldNotBeUsedOnSub()
        Dim instance As Integer = 42
        instance.ToString()
    End Sub

    <TestMethod>
    <ExpectedException(GetType(DivideByZeroException))> ' Noncompliant
    Public Function ExpectedExceptionAttrbutesShouldNotBeUsedOnFunction() As String
        Dim instance As Integer = 42
        Return instance.ToString()
    End Function

    <TestMethod>
    <ExpectedException(GetType(DivideByZeroException))> ' Compliant - one line
    Public Sub ExpectedExceptionAttrbuteAllowedForSingleLineSub()
        Throw New NotImplementedException()
    End Sub

    <TestMethod>
    <ExpectedException(GetType(DivideByZeroException))> ' Compliant - one line
    Public Function ExpectedExceptionAttrbuteAllowedForSingleLineFunction() As Integer
        Return 42
    End Function

End Class
