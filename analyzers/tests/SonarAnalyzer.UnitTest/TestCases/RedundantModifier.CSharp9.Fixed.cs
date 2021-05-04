public record Record
{
    public virtual void MyNotOverriddenMethod() { }
}

internal record PartialRecordDeclaredOnlyOnce // Fixed
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
    public override void MyOverriddenMethod() { } // Fixed

    public override int Prop { get; set; } // Fixed
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

internal record UnsafeRecord // Fixed
{
    int num;

    private delegate void MyDelegate2(int i); // Fixed

    void M() // Fixed
    {
    }

    ~UnsafeRecord() // Fixed
    {
    }
}

public record Foo
{
    public record Bar // Fixed
    {
    }

    unsafe interface MyInterface
    {
        int* Method(); // Fixed
    }

    public static void M()
    {
        checked
        {
            {
                var z = 1 + 4;
                var y = unchecked(1 +
                    4); // Fixed
            }
        }

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
            var y = 5.5 + 4; // Fixed
        }

        unchecked
        {
            var f = 5.5;
            var y = 5 + 4; // Fixed
        }

        checked
        {
            var x = (uint)10;
            var y = (int)x;
        }

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

public record RecordNewSyntax(string Input) // Fixed
{
    private string inputField = Input;
}
