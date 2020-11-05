public record Record
{
    public virtual void MyNotOverriddenMethod() { }
}

internal partial record PartialRecordDeclaredOnlyOnce // Compliant - FN
{
    void Method() { }
}

internal partial record PartialDeclaredMultipleTimes
{
}

internal partial record PartialDeclaredMultipleTimes
{
}

abstract record BaseRecord
{
    public abstract void MyOverriddenMethod();

    public abstract int Prop { get; set; }
}

sealed record SealedRecord : BaseRecord
{
    public sealed override void MyOverriddenMethod() { } // Noncompliant

    public sealed override int Prop { get; set; } // Noncompliant
}

internal record BaseRecord<T>
{
    public virtual string Process(string input)
    {
        return input;
    }
}

internal record SubRecord : BaseRecord<string>
{
    public override string Process(string input) => "Test";
}

internal unsafe record UnsafeRecord // Compliant - FN
{
    int num;

    unsafe void M() // Compliant - FN
    {
    }

    unsafe ~UnsafeRecord() // Compliant - FN
    {
    }
}

public record Foo
{
    public unsafe record Bar // Compliant - FN
    {
    }
}
