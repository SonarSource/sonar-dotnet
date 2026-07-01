public class Sample
{
    public void Method(object o, string s, int[] arr)
    {
        _ = o is not { } a;                 // Noncompliant {{Avoid declaring a variable in a negated pattern.}}
        //       ^^^^^^^^^
        _ = o is not int b;                 // Noncompliant
        _ = o is not string c;              // Noncompliant
        _ = o is not Other { } d;           // Noncompliant
        _ = s is not { Length: 0 } e;       // Noncompliant
        _ = arr is not [] g;                // Noncompliant
        if (o is not not { } h) { }         // Noncompliant
                                            // Noncompliant@-1
        _ = o is not ({ } j);               // Noncompliant
        _ = s is not { Length: var k };     // Noncompliant
        _ = s is not { Length: { } l };     // Noncompliant
        while (o is not { } i) { }          // Noncompliant

        // Compliant - no variable is declared
        _ = o is not { };
        _ = o is not int;
        _ = o is not Other;
        _ = o is not null;
        _ = o is not int _;

        // Compliant - positive patterns
        _ = o is { } positive;
        _ = o is int alsoPositive;
        _ = o is null;

        // Compliant - the 'not' constrains a value inside a subpattern; the capture itself is positive
        _ = s is { Length: not 0 } nonEmpty;
        _ = o is Other { Length: not 0 } typed;

        // Compliant - declaration sits next to a 'not', not under the 'not'
        _ = o is not int and { } chainedAnd;
        _ = s is { Length: not 0 and var lengthCapture };
        _ = o is int x and not 0;
        _ = o is not int && s is { } chainedSeparate;
        if (o is not int && s is { } chainedInIf) { }
    }
}

public class Other
{
    public int Length { get; }
}
