#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class BooleanCheckInvertedTests
    {
        public void NullableSuppression()
        {
            var a = 2;
            _ = !(a == 2); // Noncompliant {{Use the opposite operator ('!=') instead.}}
            //  ^^^^^^^^^

            _ = !(a == 2)!; // Compliant FN

            _ = !(a! == 2!); // Noncompliant
            //  ^^^^^^^^^^^
        }
    }
}
