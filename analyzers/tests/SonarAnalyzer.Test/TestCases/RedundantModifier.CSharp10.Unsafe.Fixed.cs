internal record struct UnsafeRecordStruct // Fixed
{
    int num;

    private delegate void MyDelegate2(int i); // Fixed

    void M() // Fixed
    {
    }
}

public record struct Foo
{
    public record struct Bar // Fixed
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
    }
}

public record struct RecordNewSyntax(string Input) // Fixed
{
    private string inputField = Input;
}
