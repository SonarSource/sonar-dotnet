//FN
if (false) { }
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

    public void M2() // Noncompliant [1]
                     // Secondary@-1 [1] {{+1}}
    {
        if (false) { } // Secondary [1] {{+1}}
        if (false) { } // Secondary [1] {{+1}}
        if (false) { } // Secondary [1] {{+1}}
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
        init               // Noncompliant [2]
                           // Secondary@-1 [2] {{+1}}
        {
            if (false) { } // Secondary [2] {{+1}}
            if (false) { } // Secondary [2] {{+1}}
            if (false) { } // Secondary [2] {{+1}}
        }
    }
}
