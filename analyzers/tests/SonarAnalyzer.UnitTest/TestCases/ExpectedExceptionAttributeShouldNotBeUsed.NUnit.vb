Imports System
Imports NUnit.Framework

Public Class ExceptionTests

    <Test>
    <ExpectedException(GetType(DivideByZeroException))> ' Noncompliant
    Public Sub ExpectedExceptionAttrbutesShouldNotBeUsed()
    End Sub

    <Test>
    <ExpectedException(GetType(DivideByZeroException))> ' Noncompliant
    Public Function ExpectedExceptionAttrbutesShouldNotBeUsedWithFunctions() As Integer
        Return 42
    End Function

End Class
