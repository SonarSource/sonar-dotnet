public record Record
{
    public virtual void MyNotOverriddenMethod() { }
}

internal partial record PartialRecordDeclaredOnlyOnce // Noncompliant
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

internal unsafe record UnsafeRecord // Noncompliant
{
    int num;

    private unsafe delegate void MyDelegate2(int i); // Noncompliant

    unsafe void M() // Noncompliant
    {
    }

    unsafe ~UnsafeRecord() // Noncompliant
    {
    }
}

public record Foo
{
    public unsafe record Bar // Noncompliant
    {
    }

    unsafe interface MyInterface
    {
        unsafe int* Method(); // Noncompliant
    }

    public static void M()
    {
        checked
        {
            checked // Noncompliant
//          ^^^^^^^
            {
                var z = 1 + 4;
                var y = unchecked(1 +
                    unchecked(4)); // Noncompliant
//                  ^^^^^^^^^
            }
        }

        checked // Noncompliant {{'checked' is redundant in this context.}}
        {
            var f = 5.5;
            var y = unchecked(5 + 4);
        }

        checked
        {
            var f = 5.5;
            var x = 5 + 4;
            var y = unchecked(5 + 4);
        }

        checked
        {
            var f = 5.5;
            var x = 5 + 4;
            var y = unchecked(5.5 + 4); // Noncompliant
        }

        unchecked
        {
            var f = 5.5;
            var y = unchecked(5 + 4); // Noncompliant
        }

        checked
        {
            var x = (uint)10;
            var y = (int)x;
        }

        checked // Noncompliant
        {
            var x = 10;
            var y = (double)x;
        }

        checked
        {
            var x = 10;
            x += int.MaxValue;
        }
    }
}
