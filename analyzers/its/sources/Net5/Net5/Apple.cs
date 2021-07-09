namespace Net5
{
    public sealed record Apple
    {
        public string Taste { get; init; }
        public string Color { get; init; }
        public void Deconstruct(out string x, out string y) => (x, y) = (Taste, Color);
    }
}
