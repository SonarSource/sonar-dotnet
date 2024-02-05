object a = "";
object b = "";
object n = 5;
object m = 5;
Apple apple = new();

// AND
if (n is nint) // Fixed
{
}

if (n is nint nintValue) // Fixed
{
}

if (n is Apple) // Fixed
{
}

var result = (n is string); // Fixed
result = (n is string && m is not null); // Fixed

// parenthesized pattern
result = n is ("a" or "b" or "c"); // Fixed

if (n is null && apple != null && m is null && apple is ("Sweet", "Red")) { } // FN
if (n is null && apple != null && apple is ("Sweet", "Red")) { } // FN
if (apple is ("Sweet", "Red")) { } // Fixed
if (apple is { Taste: "Sweet", Color: "Red" }) { } // Fixed
if (apple is { Taste: "Sweet", Color: "Red" }) { } // Fixed
if (apple is ("Sweet", "Red")) { } // Fixed

var x = a switch
{
    Apple appleInside => "", // Fixed
    _ => ""
};

x = a switch
{
    string s6 and (not null or "a") => s6, // FN
    _ => ""
};

x = apple switch
{
    ("Sweet", "Red") => "", // Fixed
    _ => ""
};

x = apple switch
{
    { Taste: "Sweet", Color: "Red" } => "", // Fixed
    _ => ""
};

x = apple switch
{
    { Taste: "Sweet" } and { Color: "Red" }  => "", // Fixed
    _ => ""
};

// OR, inverted
result = !(n is Apple); // Fixed
result = !(n is Apple); // Fixed
result = !(a is Apple aTyped1); // Fixed
result = !(a is Apple aTyped2); // Fixed
result = ((!((a) is Apple))); // Fixed

if (apple is not { Taste: "Sweet", Color: "Red" }) { } // Fixed
if (apple is not { Taste: "Sweet", Color: "Red" }) { } // Fixed
if (!(apple is { Taste: "Sweet", Color: "Red" })) { } // Fixed
if (apple is not ("Sweet", "Red")) { } // Fixed
if (!(apple is ("Sweet", "Red"))) { } // Fixed
if (!(apple is ("Sweet", "Red"))) { } // Fixed

x = a switch
{
    not "a" => "not a", // Fixed
    _ => "default"
};

x = a switch
{
    not Apple => "", // Fixed
    _ => ""
};

x = apple switch
{
    not ("Sweet", "Red")  => "", // Fixed
    _ => ""
};

x = apple switch
{
    not { Taste: "Sweet" } or not { Color: "Red" } => "", // Fixed
    _ => ""
};

x = apple switch
{
    not { Taste: "Sweet" } or not { Color: "Red" } => "", // Fixed
    _ => ""
};

switch (n)
{
    case "sweet": // Fixed
        break;
    case not "sweet": // Fixed
        break;
    default:
        break;
}

switch (n)
{
    case Apple appleSwitch1: // Fixed
        break;
    case not ("Sweet", "Red"): // Fixed
        break;
    default:
        break;
}

switch (n)
{
    case not Apple: // Fixed
        break;
    default:
        break;
}

// Compliant
if (apple != null && apple is not { Taste: "Sweet", Color: "Red" }) { } // Compliant
if (apple != null && apple is not ("Sweet", "Red")) { } // Compliant
if (apple is not null || apple is ("Sweet", "Red")) { } // Compliant
if (apple is not null || apple is { Taste: "Sweet", Color: "Red" }) { } // Compliant
if (apple == null || apple is { Taste: "Sweet", Color: "Red" }) { } // Compliant
if (apple is { Taste: "Sweet", Color: "Red" } || apple is null) { } // Compliant

if (n is not null && n is not null) // Compliant (not related to this rule)
{
}
if (n is not null && n != null) // Compliant (not related to this rule)
{
}
result = (n is not Apple && n != null); // Compliant
result = (a is null || a is not null); // Compliant - not related to this rule
result = (a is null || !(b is Apple)); // Compliant - a is not b
result = b is null || !(a is Apple); // Compliant - b is not a
result = (n is string s && s is not null); // Compliant, s can be null
result = (n is not Apple && n is not null); // Compliant
result = (a is null && a is string); // Compliant - rule ConditionEvaluatesToConstant should raise issue here (to be consistent with the non-C#9 tests)
x = m switch
{
    string s2 and null => s2, // Error [CS8510] The pattern is unreachable. It has already been handled by a previous arm of the switch expression or it is impossible to match.
    _ => ""
};

x = m switch
{
    string or null => "", // Compliant
    _ => ""
};

x = m switch
{
    null or string => "", // Compliant
    not null or 5 => "", // Compliant
};

x = a switch
{
    null or not null => "" // Compliant
};

x = m switch
{
    null or 5 or 7 or 8 or Apple => "", // Compliant
    string or 6 or 10 or null => "", // Compliant
    _ => ""
};

switch (n)
{
    case not null or "sweet": // Compliant
        break;
    case not null or not "sweet": // Compliant
        break;
    default:
        break;
}

switch (n)
{
    case not null or Apple: // Compliant
        break;
    default:
        break;
}

switch (n)
{
    case null or Apple: // Compliant
        break;
    default:
        break;
}

static int GetTax(object id) => id switch
{
    1 => 0, // Fixed
    not null and not 5 => 5, // Compliant
    5 => 15,
    _ => 10
};

sealed record Apple
{
    public string Taste;
    public string Color;
    public void Deconstruct(out string x, out string y) => (x, y) = (Taste, Color);
}
