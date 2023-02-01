using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.UnitTest.TestCases
{
    internal class SillyMathematicalComparison
    {
        public void Bytes()
        {
            //const sbyte smallSByte = byte.MinValue;
            //const sbyte bigSByte = byte.MaxValue;

            const byte smallByte = byte.MinValue;
            const byte bigByte = byte.MaxValue;

            const short smallShort = byte.MinValue;
            const short bigShort = byte.MaxValue;

            const ushort smallUShort = byte.MinValue;
            const ushort bigUShort = byte.MaxValue;

            const int smallInt = byte.MinValue;
            const int bigInt = byte.MaxValue;

            const uint smallUInt = byte.MinValue;
            const uint bigUInt = byte.MaxValue;

            const long smallLong = byte.MinValue;
            const long bigLong = byte.MaxValue;

            const ulong smallULong = byte.MinValue;
            const ulong bigULong = byte.MaxValue;

            //const nint smallNint = byte.MinValue;
            //const nint bigNint = byte.MaxValue;

            //const nuint smallNuint = byte.MinValue;
            //const nuint bigNuint = byte.MaxValue;

            const float smallFloat = byte.MinValue;
            const float bigFloat = byte.MaxValue;

            const double smallDouble = byte.MinValue;
            const double bigDouble = byte.MaxValue;

            const decimal smallDecimal = byte.MinValue;
            const decimal bigDecimal = byte.MaxValue;

            byte b = 42;

            _ = b <= bigByte; // Noncompliant
            _ = b <= bigShort; // Noncompliant
            _ = b >= smallByte; // Noncompliant
            _ = b >= smallShort; // Noncompliant

            _ = bigByte >= b; // Noncompliant
            _ = bigShort >= b; // Noncompliant
            _ = smallByte <= b; // Noncompliant
            _ = smallShort <= b; // Noncompliant

            _ = b >= bigByte; // Compliant, not (always true) or (always false)
            _ = b >= bigShort; // Compliant, not (always true) or (always false)
            _ = b <= smallByte; // Compliant, not (always true) or (always false)
            _ = b <= smallShort; // Compliant, not (always true) or (always false)

            _ = bigByte <= b; // Compliant, not (always true) or (always false)
            _ = bigShort <= b; // Compliant, not (always true) or (always false)
            _ = smallByte >= b; // Compliant, not (always true) or (always false)
            _ = smallShort >= b; // Compliant, not (always true) or (always false)
        }
    }
}
