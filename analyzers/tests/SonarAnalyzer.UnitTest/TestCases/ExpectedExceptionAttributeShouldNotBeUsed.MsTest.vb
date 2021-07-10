Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public Class ExceptionTests

    <TestMethod>
    <ExpectedException(GetType(DivideByZeroException))> ' Noncompliant
    Public Sub ExpectedExceptionAttrbutesShouldNotBeUsed()
    End Sub

    <TestMethod>
    <ExpectedException(GetType(DivideByZeroException))> ' Noncompliant
    Public Function ExpectedExceptionAttrbutesShouldNotBeUsedWithFunctions() As Integer
        Return 42
    End Function

End Class
