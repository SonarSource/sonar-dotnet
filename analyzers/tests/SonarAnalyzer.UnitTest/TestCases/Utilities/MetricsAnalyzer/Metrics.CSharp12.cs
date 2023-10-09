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
