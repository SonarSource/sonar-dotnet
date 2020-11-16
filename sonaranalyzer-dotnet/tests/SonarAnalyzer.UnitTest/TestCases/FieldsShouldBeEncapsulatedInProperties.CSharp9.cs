public record PublicClass
{
    public int myValue = 42; // Noncompliant {{Make this field 'private' and encapsulate it in a 'public' property.}}

    public readonly int MagicNumber = 42;
    public const int AnotherMagicNumber = 998001;

    private record PrivateClass
    {
        public int myValue = 42;
    }

    protected class ProtectedClass
    {
        public int myValue = 42; // Noncompliant
    }
}

internal record InternalClass
{
    public int myValue = 42;

    public record InnerPublicRecord
    {
        public int mySubValue = 42;
    }
}
