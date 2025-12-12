class PrimaryConstructor(bool condition /* comment */)
{
    bool Field = condition;

    int Method()
    {
        if (condition)
        {
            return 42;
        }
        return 0;
    }

    PrimaryConstructor(bool condition, int n) : this(condition) { }
}

public static class Extensions
{
    extension(string)
    {
        public static int Method()
        {
            if (true)
            {
                return 0;
            }
        }
    }
}

public class FieldKeyword
{
    public int Value
    {
        get { return field; }
        set { field = value; }
    }
}

public class NullConditionalAssignment
{
    public void Method(FieldKeyword sample)
    {
        sample?.Value = 42; 
    }
}
