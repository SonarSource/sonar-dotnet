public partial class PartialConstructor<T>
{
    public partial PartialConstructor(T param) { }
}

public partial class PartialEvent<T>  // Compliant
{
    partial event System.Action<int, T> SomeEvent
    {
        add { }
        remove { }
    }
}

public partial union PartialUnionUsedInOtherPart<T>
{
    public T Default() => default;
}

public partial union PartialUnionUnused<T> { }                  // Noncompliant
