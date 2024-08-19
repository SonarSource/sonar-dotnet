using System;

string x = "";   // FN
x = """Test2"""; // FN
Foo(x);

void UnsignedShiftRightAssignment()
{
    int i = 0;
    i >>>= 5; // Noncompliant
}

void RawStringLiterals(string param)
{
    param = """Test""";      // Noncompliant

    string x = ""; // Compliant, ignored value
    x = """Test2""";
    Foo(x);

    string y = """Test1"""; // Noncompliant
    y = """Test2""";
    Foo(y);
}

void MultilineRawStringLiterals(string param)
{
    param = """ 
        This
        is
        multiline
        """; // Noncompliant@-4

    string x = """

        """; // Compliant (empty multi-line)
    x = """
        Something
        """;

    string z = """
        Something
        """; // Noncompliant@-2
    z = """
        
        """;

    string y = """
        This
        is
        multiline
        """; // Noncompliant@-4
    y = """
        This
        is
        also
        multiline
        """;

    Foo(x);
    Foo(y);
    Foo(z);
}

void InterpolatedRawStringLiterals(string param)
{
    string aux = """Test""";
    string auxMultiline = """
        This
        is
        multiline
        """;

    param = $"""{aux}Test""";      // Noncompliant
    param = $"""{auxMultiline}Test""";      // Noncompliant
    param = $"""
        {aux}
        Test
        """;      // Noncompliant@-3
    param = $"""
        {auxMultiline}
        Test
        """;      // Noncompliant@-3

    string empty = "";
    string x = $"""{empty}"""; // Noncompliant  string interpolation values are intentionally not evaluated
    x = $"""{empty}Test""";
    Foo(x);

    string emptyMultiline = """

        """;
    string q = $"""{emptyMultiline}"""; // Noncompliant  string interpolation values are intentionally not evaluated
    q = $"""{emptyMultiline}Test""";
    Foo(q);

    string y = $"""
        Test1{aux}
        """; // Noncompliant@-2
    y = $"""
        Test2{aux}
        """;
    Foo(y);
}

void NewlinesInStringInterpolation(string param)
{
    string aux = "Test";
    param = $"{aux
        .ToUpper()}"; // Noncompliant@-1
    param = $"{aux
        .ToUpper()}";
    Foo(param);

    string empty = "";
    string x = $"{empty +
        empty}"; // Noncompliant@-1 string interpolation values are intentionally not evaluated
    x = "Test";
    Foo(x);
}

void IgnoredValues()
{
    string emptyMultilineRawStringLiteral = $$"""

        """; // Compliant
    emptyMultilineRawStringLiteral = "other";

    Foo(emptyMultilineRawStringLiteral);
}

// This method is used because the variables should be used in order for the rule to trigger.
static void Foo(object x){ }
