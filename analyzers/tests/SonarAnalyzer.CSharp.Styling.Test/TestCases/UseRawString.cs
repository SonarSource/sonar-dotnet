public class Sample
{
    public void Method()
    {
        // Noncompliant@+1 {{Use raw string literal for multiline strings.}}
        _ = @"
Verbatim
Multiline";
        // Noncompliant@+1
        _ = @"""
Still Verbatim
But mutiline
""";
        // Noncompliant@+1
        _ = @"Smallest just to get the EOL
";
        // Noncompliant@+1
        _ = @$"Verbatim
{42}
Interpolated";
        // Noncompliant@+1
        _ = $@"Interpolated
{42}
Verbatim";

        _ = """
            This is fine
            """;
        _ = $"""
            Interpolated
            {42}
            Raw
            """;
        _ = $$$"""
            Interpolated
            {{{42}}}
            Raw
            """;
    }

    public void SingleLine()
    {
        _ = "";
        _ = "Normal";
        _ = @"Verbatim";
        _ = @"""Still Verbatim""";
        _ = """Raw""";
        _ = $"Interpolated {42}";
        _ = @$"Verbatim {42} Interpolated";
        _ = $@"Interpolated {42} Verbatim";
        _ = $"""Interpolated {42} Raw""";
        _ = $$$"""Interpolated {{{42}}} Raw""";
    }

    public void Binary()
    {
        // We don't raise on these
        _ = "FirstLine" +
            "Also first Line";
        _ = "FirstLine\n" +
            "Second Line";
        _ = "FirstLine\r" +
            "Second Line";
        _ = "FirstLine\n\r" +
            "Second Line";
    }
}
