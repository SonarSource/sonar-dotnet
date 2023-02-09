using System.Diagnostics;

class SupportRawStringLiterals
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

    [DebuggerDisplay("""{NonExisting}""")] int NonExistingTripleQuotes => 1;      // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^^^^^
    [DebuggerDisplay(""""{NonExisting}"""")] int NonExistingQuadrupleQuotes => 1; // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^^^^^^^
    [DebuggerDisplay("""
        Some text{NonExisting}
        """)] int NonExistingMultiLine1 => 1;                                     // Noncompliant@-2^22#46 {{'NonExisting' doesn't exist in this context.}}
    [DebuggerDisplay("""
        Some text{Some
        Property}
        """)] int NonExistingMultiLine2 => 1;                                     // FN: the new line char make the expression within braces not a valid identifier
    [DebuggerDisplay($$"""""
        Some text{NonExisting}
        """"")] int NonExistingMultiLineInterpolated => 1;                        // FN: interpolated raw string literals strings not supported
}
