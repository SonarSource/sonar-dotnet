public record struct RecordStruct
{
    private const nint B = 5;
    public nint C = 5;
    public const nint A1 = 5; // Noncompliant {{Change this constant to a 'static' read-only property.}}

    internal record struct Nested
    {
        public const nint A = 5; // Compliant
        private const nint B = 5;
    }
}
