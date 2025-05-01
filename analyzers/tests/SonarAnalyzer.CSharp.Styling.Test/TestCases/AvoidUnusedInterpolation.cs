
public class Sample
{
    public void ToRemove()
    {
        _ = $"Unused";          // Noncompliant {{Remove unused interpolation from this string.}}
        //  ^^
        _ = $@"Unused";         // Noncompliant {{Remove unused interpolation from this string.}}
        //  ^^^
        _ = @$"Unused";         // Noncompliant {{Remove unused interpolation from this string.}}
        //  ^^^
        _ = $"""Unused""";      // Noncompliant {{Remove unused interpolation from this string.}}
        _ = $$"""Unused""";     // Noncompliant
        _ = $$$"""Unused""";    // Noncompliant
        _ = $"""
                Unused
                """;            // Noncompliant@-2
        _ = $$"""
                Unused
                """;            // Noncompliant@-2
        _ = $$$"""
                Unused
                """;            // Noncompliant@-2
    }

    public void ToReduce(int value)
    {
        _ = $$"""Too many {{value}}""";                             // Noncompliant {{Reduce the number of $ in this string.}}
        //  ^^^^^
        _ = $$"""Too many {{value}} somewhere {{42}}""";            // Noncompliant
        _ = $$$"""Too many { {{{value}}}""";                        // Noncompliant
        _ = $$$"""Too many } {{{value}}}""";                        // Noncompliant
        _ = $$$"""Too many {}{} {{{value}}}""";                       // Noncompliant
        _ = $$$"""Too many {{{value}}} somewhere {{{42}}} {}{}""";    // Noncompliant
        _ = $$$$"""Way too many } {{{{value}}}}""";                 // Noncompliant
        //  ^^^^^^^
        _ = $$"""
            Too many {{value}}
            """;                                                    // Noncompliant@-2
        _ = $$$$"""
            Way too many } {{{{value}}}}
            """;                                                    // Noncompliant@-2
        _ = $$"""{{value}}""";                                      // Noncompliant, no text

        _ = "Leave me alone";
        _ = $"Needed {value}";
        _ = $"""Needed {value}""";
        _ = $$"""Needed { {{value}}""";
        _ = $$"""Needed } {{value}}""";
        _ = $$"""Needed {}{} {{value}}""";
        _ = $$"""Needed {{value}} somewhere {{42}} {}{}""";
        _ = $$$"""Needed {{ {{{value}}}""";
        _ = $$$"""Needed }} {{{value}}}""";
        _ = $$$"""Needed {{}}{{}} {{{value}}}""";
        _ = $$$"""Needed {{{value}}} somewhere {{{42}}} {{}}{{}}""";
        _ = $"""
            Needed {value}
            """;
        _ = $$"""
            Needed } {{value}}
            """;
        _ = $"""{value}"""; // No text
        _ = $"{value}";     // No text
    }
}
