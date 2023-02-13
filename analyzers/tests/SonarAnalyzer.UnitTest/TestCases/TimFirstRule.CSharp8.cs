using System;

public class Program
{
    void PatternMatching()
    {
        var tim = new { Tim = 5 };
        if (tim is { Tim: int i })  // Noncompliant {{Expected Identifier to be 'tim' but found 'i'!}}
            //                ^
        {

        }
    }

}
