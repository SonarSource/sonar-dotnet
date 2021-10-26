using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Net6Poc.DisablingCSRFProtection
{
    internal class TestCases
    {
        public void Bar(IEnumerable<int> collection)
        {
            [IgnoreAntiforgeryToken] int Get() => 1; // Noncompliant

            _ = collection.Select([IgnoreAntiforgeryToken] (x) => x + 1); // Noncompliant

            Action a = [IgnoreAntiforgeryToken] () => { }; // Noncompliant

            Action x = true
                           ? ([IgnoreAntiforgeryToken]() => { }) // Noncompliant
                           : [IgnoreAntiforgeryToken]() => { }; // Noncompliant

            Call([IgnoreAntiforgeryToken] (x) => { }); // Noncompliant
        }

        private void Call(Action<int> action) => action(1);
    }
}
