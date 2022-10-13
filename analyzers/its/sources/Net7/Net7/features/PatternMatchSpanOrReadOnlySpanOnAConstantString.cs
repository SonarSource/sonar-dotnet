namespace Net7.features
{
    internal class PatternMatchSpanOrReadOnlySpanOnAConstantString
    {
        public bool Method(Span<char> span, ReadOnlySpan<char> readonlySpan) =>
            span is "one" || readonlySpan is "two";
    }
}
