using System;

public class StringFormatValidatorRuntimeExceptionFree
{
    void System_String_Format(string[] args)
    {
        string s;
        s = string.Format("""{0}""", 42);  // Compliant
        s = string.Format("""[0}""", 42);  // Noncompliant {{Invalid string format, unbalanced curly brace count.}}
        s = string.Format("""{{0}""", 42); // Noncompliant
    }
}
