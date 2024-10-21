// Noncompliant [0]
using System.Dynamic;

if (false) { }  // Secondary [0] {{+1}}

void TopLevelLocalFunction()
{
    if (false) // Secondary [0] {{+1}}
    { }
    if (false) // Secondary [0] {{+1}}
    { }
    if (false) // Secondary [0] {{+1}}
    { }
}

// Static local functions do not count in the overall top level statement cyclomatic complexity computation.
// They are considered methods by themselves with their own complexity score.
// See: https://github.com/SonarSource/sonar-dotnet/issues/5625
static void StaticTopLevelLocalFunction() // Noncompliant [1]
                                          // Secondary@-1 [1] {{+1}}
{
    if (false) // Secondary [1] {{+1}}
    { }
    if (false) // Secondary [1] {{+1}}
    { }
    if (false) // Secondary [1] {{+1}}
    { }
}

public record FunctionComplexity
{
    public void M1()
    {
        if (false)
        { }
        if (false)
        { }
    }

    public void M2() // Noncompliant [2]
                     // Secondary@-1 [2] {{+1}}
    {
        if (false) // Secondary [2] {{+1}}
        { }
        if (false) // Secondary [2] {{+1}}
        { }
        if (false) // Secondary [2] {{+1}}
        { }
    }

    public bool PatternMatchingAnd(object arg) =>
        //      ^^^^^^^^^^^^^^^^^^                    Noncompliant [3]
        //      ^^^^^^^^^^^^^^^^^^                    Secondary@-1 [3] {{+1}}
        arg is true
            and true
        //  ^^^                                       Secondary [3] {{+1}}
            and true
        //  ^^^                                       Secondary [3] {{+1}}
            and true;
        //  ^^^                                       Secondary [3] {{+1}}

    public bool PatternMatchingOr(object arg) =>
        //      ^^^^^^^^^^^^^^^^^                     Noncompliant [4]
        //      ^^^^^^^^^^^^^^^^^                     Secondary@-1 [4] {{+1}}
        arg is not true
            or true
        //  ^^                                        Secondary [4] {{+1}}
            or true
        //  ^^                                        Secondary [4] {{+1}}
            or true;
        //  ^^                                        Secondary [4] {{+1}}

    public int Property
    {
        get
        {
            return 0;
        }
        init               // Noncompliant [5]
                           // Secondary@-1 [5] {{+1}}
        {
            if (false)     // Secondary [5] {{+1}}
            { }
            if (false)     // Secondary [5] {{+1}}
            { }
            if (false)     // Secondary [5] {{+1}}
            { }
        }
    }
}

public struct A
{
    const string Prefix = "_";
    const string Suffix = "_";

    private (int, int) t = default;

    public A()
    {
        int a;
        (a, var b) = t;
        const string z = $"{Prefix} zzz {Suffix}";
    }
}

public struct B
{
    const string Prefix = "_";
    const string Suffix = "_";

    private (int, int) t = default;

    public B() // Noncompliant [6]
               // Secondary@-1 [6]
    {
        int a;
        if (false      // Secondary [6]
            || true    // Secondary [6]
            || false)  // Secondary [6]
        {
            (a, var b) = t;
            const string z = $"{Prefix} zzz {Suffix}";
        }
    }
}

public class LocalFunctions
{
    public void MethodWithLocalfunctions() // Noncompliant [7] {{The Cyclomatic Complexity of this method is 4 which is greater than 3 authorized.}}
                                           // Secondary@-1 [7] {{+1}}
    {
        void LocalFunction()
        {
            if (false) // Secondary [7] {{+1}}
            { }
            if (false) // Secondary [7] {{+1}}
            { }
            if (false) // Secondary [7] {{+1}}
            { }
        }

        static void StaticLocalFunctions() // Noncompliant [8] {{The Cyclomatic Complexity of this static local function is 5 which is greater than 3 authorized.}}
                                           // Secondary@-1 [8] {{+1}}
        {
            if (false) // Secondary [8] {{+1}}
            { }
            if (false) // Secondary [8] {{+1}}
            { }
            if (false) // Secondary [8] {{+1}}
            { }
            if (false) // Secondary [8] {{+1}}
            { }
        }
    }
}

public partial class PartialProperty
{
    public partial int Property { get; set; }
}
public partial class PartialProperty
{
    public partial int Property
    {
        get // Noncompliant [9] {{The Cyclomatic Complexity of this accessor is 4 which is greater than 3 authorized.}}
            // Secondary@-1 [9]
        {
            if (true      // Secondary [9]
                || false  // Secondary [9]
                || true)  // Secondary [9]
            {
                return 0;
            }
        }
        set { }
    }
}
