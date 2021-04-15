// version: CSharp9
//FNif (false) { }
if (false) { }
if (false) { }

void TopLevelLocalFunction()    // FN
{
    if (false) { }
    if (false) { }
    if (false) { }
}

public record FunctionComplexity
{
    public void M1()
    {
        if (false) { }
        if (false) { }
    }

    public void M2() // Noncompliant [3]
                     // Secondary@-1 [3] {{+1}}
    {
        if (false) { } // Secondary [3] {{+1}}
        if (false) { } // Secondary [3] {{+1}}
        if (false) { } // Secondary [3] {{+1}}
    }

    public bool PatternMatchingAnd(object arg) =>
        arg is true and true and true and true;         // FN

    public bool PatternMatchingOr(object arg) =>
        arg is true or true or true or true;            // FN

    public int Property
    {
        get
        {
            return 0;
        }
        init    // FN
        {
            if (false) { }
            if (false) { }
            if (false) { }
        }
    }
}
