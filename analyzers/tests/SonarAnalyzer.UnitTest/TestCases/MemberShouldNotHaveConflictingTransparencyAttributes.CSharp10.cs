using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Net6Poc.MemberShouldNotHaveConflictingTransparencyAttributes;

[SecurityCritical]
// Secondary@-1 [flow1]
// Secondary@-2 [flow2]
// Secondary@-3 [flow3]
// Secondary@-4 [flow4]
// Secondary@-5 [flow5]
internal class TestCases
{
    public void Bar(IEnumerable<int> collection)
    {
        [SecuritySafeCritical] int Get() => 1; // Noncompliant [flow1]

        _ = collection.Select([SecuritySafeCritical] (x) => x + 1); // Noncompliant [flow2]

        Action a =[SecuritySafeCritical] () => { }; // Noncompliant [flow3]

        Action x = true
                       ? ([SecuritySafeCritical] () => { }) // Noncompliant [flow4]
                       :[SecuritySafeCritical] () => { }; // Noncompliant [flow5]

        Call([Obsolete] (x) => { });
    }

    private void Call(Action<int> action) => action(1);
}
