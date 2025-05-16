using System;
using System.Linq;

public class Sample
{
    public void Method()
    {
        _ = """
        0
        """;           // Noncompliant {{Indent this raw string literal at line position 13.}}

        _ = """
         1
         """;          // Noncompliant {{Indent this raw string literal at line position 13.}}

        _ = """
          2
          """;         // Noncompliant {{Indent this raw string literal at line position 13.}}

        _ = """
           3
           """;        // Noncompliant {{Indent this raw string literal at line position 13.}}

        _ = """
            4
            """;

        _ = """
             5
             """;      // Noncompliant {{Indent this raw string literal at line position 13.}}
        //   ^^^

        _ = $"""
             Interpolated{5}
             """;      // Noncompliant
        _ = $$$"""
             Interpolated{{{5}}}
             """;      // Noncompliant
        _ = """
             Utf8
             """u8;   // Noncompliant

        _ = """Text""";
        _ =
        """
            Wrong start is not in scope
            """;
        _ = """
            Good
            """;
        _ = $"""{this} is not relevant""";
        _ = $$"""{{this}} is not relevant""";
    }

    public string ArrowNoncompliant() =>
    """
    Too close
    """;                // Noncompliant

    public string ArrowCompliant() =>
        """
        Good
        """;

    public object ArrowInInvocationArgument() =>
        Invocation("""
            Good
            """);

    public object ArrowInConstructorArgument() =>
        new Exception("""
            Good
            """);

    public Exception ArrowInConstructorArgumentImplicit() =>
        new("""
            Good
            """);

    public string ReturnNoncompliant()
    {
        return """
        Too close
        """;            // Noncompliant
    }

    public string ReturnCompliant()
    {
        return """
            Good
            """;
    }

    public string ArrowPropertyNoncompliant
    {
        get => """
        Too close
        """;    // Noncompliant
    }
    public string ArrowPropertyCompliant
    {
        get => """
            Good
            """;
    }

    public void Invocations()
    {
        Invocation("""
        Too close
        """,                // Noncompliant {{Indent this raw string literal at line position 13.}}
            """
            Good, standalone start line
            """, """
            Good, sharing start line with comma
            """, """
                Too far
                """);       // Noncompliant
        Invocation("""
            Good
            """, """
            Good
            """);
        global::Sample.Invocation("""
        Too close
        """,            // Noncompliant {{Indent this raw string literal at line position 13.}}
            """
            Good
            """, """
                 Too far
                 """);     // Noncompliant
        global::Sample.Invocation("""
            Good
            """);
        // This is bad already for other reasons
        Invocation(Invocation(Invocation("""
                            Too close
                            """,            // Noncompliant {{Indent this raw string literal at line position 33.}}
                                """
                                Good
                                """, """
                                    Too far
                                    """))); // Noncompliant
        // This is bad already for other reasons
        Invocation(Invocation(Invocation("""
                                Good
                                """)));

        Invocation(
            Invocation("""
            Nested too close
            """,         // Noncompliant {{Indent this raw string literal at line position 17.}}
                    """
                    Nested too far
                    """),  // Noncompliant
            "Some other argument");
        Invocation(
            Invocation("""
                Good
                """, """
                Good
                """),
            "Some other argument");
        global::Sample.Invocation(  // Longer invocation name does not matter
            """
            Good
            """, """
            Good
            """);

        Invocation(Invocation(Invocation(   // This is bad already for other reasons
                                """
                                Good
                                """, """
                                Good
                                """)));
    }

    public void Lambdas(int[] list)
    {
        // Simple lambda
        list.Where(x => """
        We don't care, it's wrong already to use it here
        """ == "");
        list.Where(x => """
                                        We don't care, it's wrong already to use it here
                                        """ == "");
        // Parenthesized lambda
        list.Where((x) => """
        We don't care, it's wrong already to use it here
        """ == "");
        list.Where((x) => """
                                        We don't care, it's wrong already to use it here
                                        """ == "");

        list.Where(x =>
        {
            return """
            Too close
            """ == "";          // Noncompliant
            return """
                Good
                """ == "";
            return """
                    Too far
                    """ == "";  // Noncompliant
        });
    }

    public void If()
    {
        if ("""
        We don't care, it's wrong already to use it here
        """ == """
                            We don't care, it's wrong already to use it here
                            """)
        {
            _ = """
            Alignment inside IF body is supported
            """;    // Noncompliant
        }
        if (true)
            _ = """
        Too close, even without parenthesis
        """;        // Noncompliant
    }

    public void While()
    {
        while ("""
        We don't care, it's wrong already to use it here
        """ == """
                        We don't care, it's wrong already to use it here
                        """)
        {
            _ = """
            Alignment inside WHILE body is supported
            """;    // Noncompliant
        }
    }

    public void For()
    {
        for (var i = 0; """
            We don't care, it's wrong already to use it here
            """ == """
                                    We don't care, it's wrong already to use it here
                                    """; i++)
        {
            _ = """
            Alignment inside FOR body is supported
            """;    // Noncompliant
        }
    }

    public static bool Invocation(params object[] args) => true;

    [Obsolete("""
        For coverage
        """)]
    public void Coverage() { }
}
