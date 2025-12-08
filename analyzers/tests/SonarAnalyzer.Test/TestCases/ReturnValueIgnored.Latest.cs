using System;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Frozen;

class CSharp13
{
    // https://sonarsource.atlassian.net/browse/NET-416
    void ReadOnlySet(HashSet<int> set)
    {
        var readonlySet = new ReadOnlySet<int>(set);
        readonlySet.OfType<object>();   // Noncompliant
        readonlySet.ToList();           // Noncompliant
        readonlySet.Where(i => true);   // Noncompliant
        readonlySet.Min();              // Noncompliant
        readonlySet.ToHashSet();        // Noncompliant
        readonlySet.ToImmutableArray(); // Noncompliant
        readonlySet.ToFrozenSet();      // Noncompliant
    }

    void LinqExtensions(List<int> list)
    {
        list.CountBy(i => i);                              // Noncompliant
        list.AggregateBy(x => x, y => y, (x, y) => x + y); // Noncompliant
        list.Index();                                      // Noncompliant
    }
}

class CSharp14
{
    private IEnumerable<int> field;

    void NullConditionalAssignment(CSharp14 sample, List<int> list)
    {
        sample?.field = list.Where(x => x > 5); // Compliant
    }
}
