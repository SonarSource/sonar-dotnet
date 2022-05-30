// Noncompliant [0]
if (false) { }  // Secondary [0] {{+1}}

void TopLevelLocalFunction()
{
    if (false) { }  // Secondary [0] {{+1}}
    if (false) { }  // Secondary [0] {{+1}}
    if (false) { }  // Secondary [0] {{+1}}
}

static void StaticTopLevelLocalFunction() // Compliant, static local functions do not count in the overall top level statement complexity computation.
                                          // They are considered methods by themselves with their own complexity score.
                                          // See: https://github.com/SonarSource/sonar-dotnet/issues/5625
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
        //      ^^^^^^^^^^^^^^^^^^                    Noncompliant [2]
        //      ^^^^^^^^^^^^^^^^^^                    Secondary@-1 [2] {{+1}}
        arg is true
            and true
        //  ^^^                                       Secondary [2] {{+1}}
            and true
        //  ^^^                                       Secondary [2] {{+1}}
            and true;
        //  ^^^                                       Secondary [2] {{+1}}

    public bool PatternMatchingOr(object arg) =>
        //      ^^^^^^^^^^^^^^^^^                     Noncompliant [3]
        //      ^^^^^^^^^^^^^^^^^                     Secondary@-1 [3] {{+1}}
        arg is not true
            or true
        //  ^^                                        Secondary [3] {{+1}}
            or true
        //  ^^                                        Secondary [3] {{+1}}
            or true;
        //  ^^                                        Secondary [3] {{+1}}

    public int Property
    {
        get
        {
            return 0;
        }
        init               // Noncompliant [4]
                           // Secondary@-1 [4] {{+1}}
        {
            if (false) { } // Secondary [4] {{+1}}
            if (false) { } // Secondary [4] {{+1}}
            if (false) { } // Secondary [4] {{+1}}
        }
    }
}
