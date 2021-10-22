using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Cors;

namespace Net6Poc.PermissiveCors
{
    internal class TestCases
    {
        public void Bar(IEnumerable<int> collection)
        {
            [EnableCors()] int Get() => 1; // Compliant - we don't know what default policy is

            _ = collection.Select([EnableCors("policyName")] (x) => x + 1);

            Action a = [EnableCors("*")] () => { }; // Compliant - `*`, in this case, is the name of the policy

            Action x = true
                           ? ([EnableCors] () => { })
                           : [EnableCors("*")] () => { };

            Call([EnableCors] (x) => { });
        }

        private void Call(Action<int> action) => action(1);
    }
}
