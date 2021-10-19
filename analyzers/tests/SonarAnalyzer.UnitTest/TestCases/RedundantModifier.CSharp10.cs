internal unsafe record struct UnsafeRecordStruct // FN
{
    int num;

    private unsafe delegate void MyDelegate2(int i); // FN

    unsafe void M() // FN
    {
    }
}

public record struct Foo
{
    public unsafe record struct Bar // FN
    {
    }

    unsafe interface MyInterface
    {
        unsafe int* Method(); // FN
    }

    public static void M()
    {
        checked
        {
            checked // Noncompliant
            {
                var z = 1 + 4;
                var y = unchecked(1 +
                    unchecked(4)); // Noncompliant
            }
        }

        checked // Noncompliant
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

public unsafe record struct RecordNewSyntax(string Input) // FN
{
    private string inputField = Input;
}
