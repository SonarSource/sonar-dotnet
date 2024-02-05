Imports System
Imports NUnit.Framework

Public Class ExceptionTests

    ' Noncompliant@+2
    <Test>
    <ExpectedException(GetType(DivideByZeroException))>
    Public Sub ExpectedExceptionAttrbutesShouldNotBeUsedOnSub()
        Dim instance As Integer = 42
        instance.ToString()
    End Sub

    ' Noncompliant@+2
    <Test>
    <ExpectedException(GetType(DivideByZeroException))>
    Public Function ExpectedExceptionAttrbutesShouldNotBeUsedOnFunction() As String
        Dim instance As Integer = 42
        Return instance.ToString()
    End Function

    ' Compliant - one line
    <Test>
    <ExpectedException(GetType(DivideByZeroException))>
    Public Sub ExpectedExceptionAttrbuteAllowedForSingleLineSub()
        Throw New NotImplementedException()
    End Sub

    ' Compliant - one line
    <Test>
    <ExpectedException(GetType(DivideByZeroException))>
    Public Function ExpectedExceptionAttrbuteAllowedForSingleLineFunction() As Integer
        Return 42
    End Function

End Class

Public Interface ISomeInterface
    Sub SomeSub()
End Interface

Public MustInherit Class AbstractClass
    Protected MustOverride Function AbstractFunction() As Boolean
End Class
