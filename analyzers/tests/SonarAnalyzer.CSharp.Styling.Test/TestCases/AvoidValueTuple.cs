using System;

public class Sample
{
    public (string, int) ReturnedType() // Noncompliant {{Do not use ValueTuple in the production code due to missing System.ValueTuple.dll.}}
    //     ^^^^^^^^^^^^^
    {
        return ("Lorem", 42);   // Noncompliant
        //     ^^^^^^^^^^^^^
    }

    public (string Name, int Count) NamedReturnedType() =>  // Noncompliant
        ("Lorem", 42);                                      // Noncompliant

    public void Use()
    {
        (string, int) noName;                   // Noncompliant
        (string Name, int Count) withName;      // Noncompliant

        noName = ("Lorem", 42);     // Noncompliant
        withName = ("Lorem", 42);   // Noncompliant
    }

    public System.Tuple<string, int> ExplicitType() // Complaint
    {
        return new Tuple<string, int>("Lorem", 42);
    }

    public int SwitchExpression(int a, int b)
    {
        var result = (a, b) switch      // Noncompliant: a ValueTuple is created for the SwitchExpressionException that is thrown for uncovered cases
        {
            (0, 0) => 0,
            (1, 1) => 2,
            _ => 42
        };
        return result;
    }

    public int SwitchStatement(int a, int b)
    {
        switch (a, b)                   // Noncompliant: unlike switch expressions, switch statements use ValueTuple under the hood
        {
            case (0, 0):
                return 0;
            case (1, 1):
                return 1;
            default:
                return 42;
        };
    }

    public int PositionalPatternMatching(Sample s) => s switch
    {
        (0, 0) => 0, // Compliant, Sample.Deconstruct is called
        _ => 42
    };

    void Deconstruct(out int i, out int j)
    {
        i = 42; j = 12;
    }
}
