namespace CSharpLatest.CSharp11Features;

internal class RawStringLiterals
{
    public string Method(decimal longitude, decimal latitude)
    {
        string longMessage = """
This is a long message.
It has several lines.
    Some are indented
            more than others.
Some should start at the first column.
Some have "quoted text" in them.
""";

        return $$"""
    You are at {{{longitude}}, {{latitude}}}
    """;
    }
}
