using System;
using System.Collections.Generic;
//using System.Collections.Immutable;
using System.Linq;

public class Program
{
    static void Main()
    {
        var set = new SortedSet<int>();
        //var immutable = ImmutableSortedSet.Create<int>();
        //var builder = ImmutableSortedSet.CreateBuilder<int>();

        _ = set.Min; // Compliant
        _ = set.Max; // Compliant

        _ = set.Min(); // Noncompliant
        _ = set.Max(); // Noncompliant
    }
}
