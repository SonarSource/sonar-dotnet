record UseValueParameter
{
    int count;

    public int Count
    {
        get { return count; }
        init { count = 3; } // Noncompliant {{Use the 'value' parameter in this property init accessor declaration.}}
    }

    public int Count2
    {
        get { return count; }
        set  // Noncompliant
        {
            void Foo(int value) {
                count = value;
            }
        }
    }

    public int Count3
    {
        get => 42;
        set  // Noncompliant
        {
            System.Func<int, int> f = value => value;
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
