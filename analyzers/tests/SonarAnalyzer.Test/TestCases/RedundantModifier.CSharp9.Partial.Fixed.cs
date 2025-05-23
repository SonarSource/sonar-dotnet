﻿public record Record
{
    public virtual void MyNotOverriddenMethod() { }
}

internal record PartialRecordDeclaredOnlyOnce // Fixed
{
    void Method() { }
}

internal record PartialPositionalRecordDeclaredOnlyOnce(string parameter) // Fixed
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
    public sealed override void MyOverriddenMethod() { } // Fixed

    public sealed override int Prop { get; set; } // Fixed
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

internal unsafe record UnsafeRecord // Fixed
{
    int num;

    private unsafe delegate void MyDelegate2(int i); // Fixed

    unsafe void M() // Fixed
    {
    }

    unsafe ~UnsafeRecord() // Fixed
    {
    }
}

public record Foo
{
    public unsafe record Bar // Fixed
    {
    }

    unsafe interface MyInterface
    {
        unsafe int* Method(); // Fixed
    }

    public static void M()
    {
        checked
        {
            checked // Fixed
            {
                var z = 1 + 4;
                var y = unchecked(1 +
                    unchecked(4)); // Fixed
            }
        }

        checked // Fixed
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
            var y = unchecked(5.5 + 4); // Fixed
        }

        unchecked
        {
            var f = 5.5;
            var y = unchecked(5 + 4); // Fixed
        }

        checked
        {
            var x = (uint)10;
            var y = (int)x;
        }

        checked // Fixed
        {
            var x = 10;
            var y = (double)x;
        }

        checked
        {
            var x = 10;
            x += int.MaxValue;
        }

        checked // Fixed
        {
            nint x = 42;
            nuint y = 42;

            x += 42;
            y += 42;
        }
    }
}

public unsafe record RecordNewSyntax(string Input) // Fixed
{
    private string inputField = Input;
}
