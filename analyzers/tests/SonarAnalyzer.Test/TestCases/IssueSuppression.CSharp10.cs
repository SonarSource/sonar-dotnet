using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Net6Poc.IssueSuppression
{
    internal class TestCases
    {
        public void Bar(IEnumerable<int> collection)
        {
            [SuppressMessage("", "")] int Get() => 1; // Noncompliant

            _ = collection.Select([SuppressMessage("", "")] (x) => x + 1); // Noncompliant

            Action a =[SuppressMessage("", "")] () => { }; // Noncompliant

            Action x = true
                           ? ([SuppressMessage("", "")] () => { }) // Noncompliant
                           :[SuppressMessage("", "")] () => { }; // Noncompliant

            Call([SuppressMessage("", "")] (x) => { }); // Noncompliant
        }

        private void Call(Action<int> action) => action(1);
    }
}
