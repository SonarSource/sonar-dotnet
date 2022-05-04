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

public record struct RecordNewSyntax(string Input) // Fixed
{
    private string inputField = Input;
}
