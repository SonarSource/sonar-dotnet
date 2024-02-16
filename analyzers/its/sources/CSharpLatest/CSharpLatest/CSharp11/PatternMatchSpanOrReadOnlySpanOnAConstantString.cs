namespace CSharpLatest.CSharp11
{
    internal class PatternMatchSpanOrReadOnlySpanOnAConstantString
    {
        public bool Method(Span<char> span, ReadOnlySpan<char> readonlySpan) =>
            span is "one" || readonlySpan is "two";
    }
}
