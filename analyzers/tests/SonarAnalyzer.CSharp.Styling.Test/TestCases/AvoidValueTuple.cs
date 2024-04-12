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
}
