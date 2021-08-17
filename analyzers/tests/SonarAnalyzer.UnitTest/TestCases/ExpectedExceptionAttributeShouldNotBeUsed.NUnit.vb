Imports System
Imports NUnit.Framework

Public Class ExceptionTests

    <Test>
    <ExpectedException(GetType(DivideByZeroException))> ' Noncompliant
    Public Sub ExpectedExceptionAttrbutesShouldNotBeUsedOnSub()
        Dim instance As Integer = 42
        instance.ToString()
    End Sub

    <Test>
    <ExpectedException(GetType(DivideByZeroException))> ' Noncompliant
    Public Function ExpectedExceptionAttrbutesShouldNotBeUsedOnFunction() As String
        Dim instance As Integer = 42
        Return instance.ToString()
    End Function

    <Test>
    <ExpectedException(GetType(DivideByZeroException))> ' Compliant - one line
    Public Sub ExpectedExceptionAttrbuteAllowedForSingleLineSub()
        Throw New NotImplementedException()
    End Sub

    <Test>
    <ExpectedException(GetType(DivideByZeroException))> ' Compliant - one line
    Public Function ExpectedExceptionAttrbuteAllowedForSingleLineFunction() As Integer
        Return 42
    End Function

End Class
