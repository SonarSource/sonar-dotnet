using System;

public class Program
{
    void PatternMatching()
    {
        if ("" is { Length: var length })   // Noncompliant {{Tim's first rule!}}
        {

        }
    }
}
