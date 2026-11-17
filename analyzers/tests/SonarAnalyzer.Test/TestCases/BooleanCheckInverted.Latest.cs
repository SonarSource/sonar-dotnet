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
            _ = !(a < b);   // Compliant, Decimal32 can be NaN https://sonarsource.atlassian.net/browse/NET-4890
            _ = !(c < d);   // Compliant, BFloat16 can be NaN https://sonarsource.atlassian.net/browse/NET-4890
            _ = !(a == b);  // Noncompliant {{Use the opposite operator ('!=') instead.}}
            _ = !(c != d);  // Noncompliant {{Use the opposite operator ('==') instead.}}
        }

        public void OtherIeeeTypes(System.Numerics.Decimal64 a, System.Numerics.Decimal64 b, System.Numerics.Decimal128 c, System.Numerics.Decimal128 d, decimal m, decimal n)
        {
            _ = !(a <= b);  // Compliant, Decimal64 can be NaN
            _ = !(5 > a);   // Compliant, the right operand Decimal64 can be NaN
            _ = !(c >= d);  // Compliant, Decimal128 can be NaN
            _ = !(a == b);  // Noncompliant {{Use the opposite operator ('!=') instead.}}
            _ = !(m < n);   // Noncompliant {{Use the opposite operator ('>=') instead.}}
        }

        public void Generic<TFloat, TBinaryFloat, TInteger>(TFloat a, TFloat b, TBinaryFloat c, TBinaryFloat d, TInteger e, TInteger f)
            where TFloat : System.Numerics.IFloatingPointIeee754<TFloat>
            where TBinaryFloat : System.Numerics.IBinaryFloatingPointIeee754<TBinaryFloat>
            where TInteger : System.Numerics.IBinaryInteger<TInteger>
        {
            _ = !(a < b);   // Compliant, TFloat can be NaN
            _ = !(c > d);   // Compliant, TBinaryFloat can be NaN
            _ = !(e < f);   // Noncompliant {{Use the opposite operator ('>=') instead.}}
        }

        public void TransitiveConstraint<TFloat, TDerivedFloat, TInteger, TDerivedInteger>(TDerivedFloat a, TDerivedFloat b, TDerivedInteger c, TDerivedInteger d)
            where TFloat : System.Numerics.IFloatingPointIeee754<TFloat>
            where TDerivedFloat : TFloat
            where TInteger : System.Numerics.IBinaryInteger<TInteger>
            where TDerivedInteger : TInteger
        {
            _ = !(a < b);   // Compliant, TDerivedFloat is constrained by TFloat which can be NaN
            _ = !(c < d);   // Noncompliant {{Use the opposite operator ('>=') instead.}}
        }
    }
}
