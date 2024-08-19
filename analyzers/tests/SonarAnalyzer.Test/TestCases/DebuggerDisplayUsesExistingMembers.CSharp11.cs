using System.Diagnostics;

class RawStringLiterals
{
    int SomeProperty => 1;
    int SomeField = 2;

    [DebuggerDisplay("""{SomeProperty}""")] int ExistingMemberTripleQuotes => 1;
    [DebuggerDisplay(""""{SomeField}"""")] int ExistingMemberQuadrupleQuotes => 1;
    [DebuggerDisplay("""
        Some text{SomeField}
        """)] int ExistingMultiLine => 1;
    [DebuggerDisplay($$"""""
        Some text{SomeField}
        """"")] int ExistingMultiLineInterpolated => 1;

    [DebuggerDisplay("""{Nonexistent}""")] int NonexistentTripleQuotes => 1;      // Noncompliant
    //               ^^^^^^^^^^^^^^^^^^^
    [DebuggerDisplay(""""{Nonexistent}"""")] int NonexistentQuadrupleQuotes => 1; // Noncompliant
    //               ^^^^^^^^^^^^^^^^^^^^^
    [DebuggerDisplay("""
        Some text{Nonexistent}
        """)] int NonexistentMultiLine1 => 1;                                     // Noncompliant@-2^22#46
    [DebuggerDisplay("""
        Some text{Some
        Property}
        """)] int NonexistentMultiLine2 => 1;                                     // FN: the new line char make the expression within braces not a valid identifier
    [DebuggerDisplay($$"""""
        Some text{Nonexistent}
        """"")] int NonexistentMultiLineInterpolated => 1;                        // FN: interpolated raw string literals strings not supported
}
