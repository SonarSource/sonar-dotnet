namespace CSharpLatest.CSharp11Features;

internal class PatternMatchSpanOrReadOnlySpanOnAConstantString
{
    public bool Method(Span<char> span, ReadOnlySpan<char> readonlySpan) =>
        span is "one" || readonlySpan is "two";
}
