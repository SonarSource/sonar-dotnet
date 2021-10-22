using System;
using System.Collections.Generic;
using System.Linq;

namespace Net6Poc.ObsoleteAttributesNeedExplanation;

internal class TestCases
{
    public void Bar(IEnumerable<int> collection)
    {
        [Obsolete] int Get() => 1; // Noncompliant

        _ = collection.Select([Obsolete] (x) => x + 1); // Noncompliant

        Action a = [Obsolete] () => { }; // Noncompliant

        Action x = true
                       ? ([Obsolete] () => { }) // Noncompliant
                       :[Obsolete] () => { }; // Noncompliant

        Call([Obsolete("something")] (x) => { });
    }

    private void Call(Action<int> action) => action(1);
}
