using System;

public class ExceptionTests
{
    [ExpectedException] // Noncompliant {{Use an Assert method to test the thrown exception.}}
//   ^^^^^^^^^^^^^^^^^
    public void ExpectedExceptionAttrbutesShouldNotBeUsed() { }

    [ExpectedException(typeof(DivideByZeroException))] // Noncompliant
    public void ExpectedExceptionAttrbutesShouldNotBeUsedWithType() { }
}

public class ExpectedExceptionAttribute : Attribute
{
    public ExpectedExceptionAttribute(Type expectedType = null)
    {
        ExceptionType = expectedType;
    }
    public Type ExceptionType { get; }
}
