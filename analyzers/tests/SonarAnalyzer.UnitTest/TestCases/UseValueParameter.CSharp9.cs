record UseValueParameter
{
    int count;

    public int Count
    {
        get { return count; }
        init { count = 3; } // Noncompliant
    }

    public int Count2
    {
        get { return count; }
        set {
            void Foo(int value) {
                count = value;
            }
        }
    }

    public string FirstName { get; init; } = "Foo";

    public string LastName { get => string.Empty; init { } } // Noncompliant

    public int this[int i]
    {
        get => 0;
        init // Noncompliant
        {
            var x = 1;
        }
    }
}
