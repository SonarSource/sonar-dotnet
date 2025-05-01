
using System;

public class Sample
{
    private Exception field;

    public Sample() => field = new Exception();     // Noncompliant {{Move this expression body to the next line.}}
    //                 ^^^^^^^^^^^^^^^^^^^^^^^

    public object MemberAccess() => field.Message;  // Noncompliant {{Move this expression body to the next line.}}
    //                              ^^^^^^^^^^^^^

    public object Invocation() => Expression();     // Noncompliant
    public object Expression() => 1 + 2;            // Noncompliant
    ~Sample() => Console.WriteLine("Bye");          // Noncompliant

    public static object operator +(Sample a, Sample b) => a.field;                 // Noncompliant
    public static explicit operator string(Sample sample) => sample.field.Message;  // Noncompliant

    // Single-token values are tolerated
    public object SingleName() => field;    // Comments are fine
    public object Simple_Null() => null;
    public object Simple_Bool() => true;
    public object Simple_Int() => 42;
    public object Simple_Decimal() => 42.42D;

    public bool ComplexCompliant() =>
        true && false || true;

    public void WithBody()
    {
        Console.WriteLine("This is fine");
    }
}

public interface IBase
{
    object FromInterface();
}

public abstract class Base
{
    public abstract object FromAbstractClass();
    public virtual object FromVirtualMethod() => 42;
}

public class TestStub : Base, IBase
{
    public object NewMethod() => throw new NotImplementedException();   // Noncompliant, not a mandatory stub override

    // This is common pattern in UTs => we ignore these specific cases
    public override object FromAbstractClass() => throw new NotImplementedException();
    public override object FromVirtualMethod() => throw new NotSupportedException("Parameters are allowed");
    public object FromInterface() => throw new NotImplementedException();
}

public class UnexpectedExceptionType : IBase
{
    public object FromInterface() => throw new ArgumentException("This is functional"); // Noncompliant
}
