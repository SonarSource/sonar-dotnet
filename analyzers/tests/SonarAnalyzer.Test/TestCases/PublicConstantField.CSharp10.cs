public record struct Record
{
    public Record() { }

    private const int B = 5;
    public int C = 5;
    public const int A1 = 5; // Noncompliant {{Change this constant to a 'static' read-only property.}}

    internal record Nested
    {
        public Nested() { }

        public const int A = 5; // Compliant
        private const int B = 5;
    }
}

