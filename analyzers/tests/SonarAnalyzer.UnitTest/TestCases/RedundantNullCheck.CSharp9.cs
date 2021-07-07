object a = "";
object b = "";
object n = 5;
object m = 5;
Apple apple = new();

// AND
if (n != null && n is nint) // Noncompliant
{
}

if (n is not null && n is nint nintValue) // Noncompliant
{
}

if (!(n is null) && n is Apple) // FN
{
}

var result = (n is string && n is not null); // Noncompliant
result = (n is string && n is not null && m is not null); // Noncompliant
// parenthesized pattern
result = n is ("a" or "b" or "c") && n is not null; // Noncompliant

if (apple != null && apple is ("Sweet", "Red")) { } // Noncompliant
if (apple != null && apple is { Taste: "Sweet", Color: "Red" }) { } // Noncompliant
if (!(apple is null) && apple is { Taste: "Sweet", Color: "Red" }) { } // FN
if (apple is not null && apple is ("Sweet", "Red")) { } // Noncompliant

var x = a switch
    {
        Apple appleInside and not null => "", // Noncompliant
        _ => ""
    };

x = a switch
{
    string s6 and (not null or "a") => s6, // FN
    _ => ""
};

x = apple switch
{
    not null and ("Sweet", "Red") => "", // Noncompliant
    _ => ""
};

x = apple switch
{
    { Taste: "Sweet", Color: "Red" } and not null => "", // Noncompliant
    _ => ""
};


// OR, inverted
result = n is null || !(n is Apple); // Noncompliant {{Remove this unnecessary null check; 'is' returns false for nulls.}}
result = !(n is Apple) || n is null; // Noncompliant
result = a is null || !(a is Apple aTyped1); // Noncompliant
result = !(a is Apple aTyped2) || a == null; // Noncompliant
result = ((!((a) is Apple))) || ((a) == (null)); // Noncompliant

if (apple == null || apple is not { Taste: "Sweet", Color: "Red" }) { } // FN
if (apple == null || !(apple is { Taste: "Sweet", Color: "Red" })) { } // Noncompliant
if (apple == null || apple is not ("Sweet", "Red")) { } // FN
if (apple == null || !(apple is ("Sweet", "Red"))) { } // Noncompliant

x = a switch
{
    null or not "a" => "", // Noncompliant
    _ => ""
};

x = a switch
{
    null or not Apple => "", // Noncompliant
    _ => ""
};

x = apple switch
{
    null or not ("Sweet", "Red")  => "", // Noncompliant
    _ => ""
};

x = apple switch
{
    null or not { Taste: "Sweet", Color: "Red" } => "", // Noncompliant
    _ => ""
};

// Compliant
if (apple != null && apple is not { Taste: "Sweet", Color: "Red" }) { } // Compliant
if (apple != null && apple is not ("Sweet", "Red")) { } // Compliant
if (apple is not null || apple is ("Sweet", "Red")) { } // Compliant
if (apple is not null || apple is { Taste: "Sweet", Color: "Red" }) { } // Compliant


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
        string s2 and null => s2, // // Compliant - rule ConditionEvaluatesToConstant will raise issue here
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

x = m switch
{
    null or string or 5 or 7 or 8 or Apple => "", // Compliant
    _ => ""
};

static int GetTax(object id) => id switch
{
    not null and 1 => 0, // Noncompliant
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
