using System;

public class StringFormatValidatorTypoFree
{
    void Test()
    {
        string arg0 = string.Empty;
        string arg1 = string.Empty;

        var s = string.Format("""some text"""); // Noncompliant {{Remove this formatting call and simply use the input string.}}
        s = string.Format(null, 42);                //Noncompliant {{Invalid string format, the format string cannot be null.}}
        s = string.Format(string.Format("""foo"""));    //Noncompliant
        s = string.Format("""{0}""", arg0, arg1); // Noncompliant {{The format string might be wrong, the following arguments are unused: 'arg1'.}}
    }
}
