using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

class NonCompliant
{
    public void Method(IEnumerable<int> collection)
    {
        [ExcludeFromCodeCoverage] int Get() => 1; // Noncompliant

        _ = collection.Select([ExcludeFromCodeCoverage] (x) => x + 1); // Noncompliant

        Action a = [ExcludeFromCodeCoverage] () => { }; // Noncompliant

        Action x = true
            ? ([ExcludeFromCodeCoverage] () => { }) // Noncompliant
            : [ExcludeFromCodeCoverage] () => { }; // Noncompliant

        Call([ExcludeFromCodeCoverage(Justification = "justification")] (x) => { });
    }

    private void Call(Action<int> action) => action(1);
}
