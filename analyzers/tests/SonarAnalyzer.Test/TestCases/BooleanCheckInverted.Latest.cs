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

namespace CSharp15
{
    public class NumericTypes
    {
        public void Test(System.Numerics.Decimal32 a, System.Numerics.Decimal32 b, System.Numerics.BFloat16 c, System.Numerics.BFloat16 d)
        {
            _ = !(a < b);   // Noncompliant {{Use the opposite operator ('>=') instead.}} FP NET-4890
            _ = !(c < d);   // Noncompliant {{Use the opposite operator ('>=') instead.}} FP NET-4890
        }
    }
}
