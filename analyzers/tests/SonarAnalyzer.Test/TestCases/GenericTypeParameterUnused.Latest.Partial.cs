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
