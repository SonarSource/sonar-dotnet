Imports System

Public Class ExceptionTests

    <ExpectedException()> 'Noncompliant {{Use an Assert method to test the thrown exception.}}
    Public Sub ExpectedExceptionAttrbutesShouldNotBeUsed()
    End Sub

    <ExpectedException(GetType(DivideByZeroException))> ' Noncompliant
    Public Sub ExpectedExceptionAttrbutesShouldNotBeUsedWithType()
    End Sub
End Class

Public Class ExpectedExceptionAttribute
    Inherits Attribute
    Public Sub New()
    End Sub
    Public Sub New(expectedType As Type)
        ExceptionType = expectedType
    End Sub
    Public Property ExceptionType As Type
End Class
