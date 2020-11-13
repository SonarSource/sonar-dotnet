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

    public void M2() // Noncompliant [3]
                     // Secondary@-1 [3] {{+1}}
    {
        if (false) { } // Secondary [3] {{+1}}
        if (false) { } // Secondary [3] {{+1}}
        if (false) { } // Secondary [3] {{+1}}
    }

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
