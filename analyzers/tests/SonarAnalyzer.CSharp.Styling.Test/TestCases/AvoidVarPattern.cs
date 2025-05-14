using System.Linq;

public class Sample
{
    private string name;
    private int min;

    public void Method()
    {
        if (Invocation() is var value       // Noncompliant {{Avoid embedding this var pattern. Declare it in var statement instead.}}
        //                  ^^^^^^^^^
            && value.Name == name
            && value.Count > min)
        { }

        if (Invocation() is var single)      // Noncompliant
        { }

        if (Invocation() is { } prerequisite
            && Invocation() is var afterPrerequisite    // Compliant, can not be extracted without adding more nesting
            && afterPrerequisite.Name == name
            && afterPrerequisite.Count > min)
        { }

        if (true
            && Invocation() is var middle
            && middle.Name == name)
        { }

        if (true
            && Invocation() is var last)    // Noncompliant
        { }

        if (Invocation() is var first       // Noncompliant
            && Invocation() is var second   // Bad, but we don't raise. Once the previous one is fixed, this will light up
            && first.Name == name
            && first.Count > min
            && second.Name == name
            && second.Count > min)
        { }
        else if (Invocation() is var unavoidable
            && unavoidable.Name == name
            && unavoidable.Count > min)
        { }

        while (Invocation() is var inWhile  // Noncompliant
            && inWhile.Name == name
            && inWhile.Count > min)
        { }

        for (var i = 0; Invocation() is var inFor && inFor.Name == name && inFor.Count > min; i++)  // Noncompliant
        //                              ^^^^^^^^^
        { }

        var compliant = Invocation();       // Compliant
        if (compliant.Name == name && compliant.Count > min)
        { }

        if ((Invocation() is var parenthesized))   // We don't raise, some more tricky logic is going on around
        { }

        _ = Invocation() is { Name: var declaredName }; // T0035
    }

    public bool Arrow() =>
        Invocation() is var value       // Compliant
        && value.Name == name
        && value.Count > min
        && Invocation() is var last;    // Useless, but compliant

    public void Lambda(Pair[] list)
    {
        list.Where(x => x.Inc() is var value // Compliant
                        && value.Name == name
                        && value.Count > min);
    }

    private static Pair Invocation() => null;
}

public record Pair(string Name, int Count)
{
    public Pair Inc() => new(Name, Count + 1);
}
