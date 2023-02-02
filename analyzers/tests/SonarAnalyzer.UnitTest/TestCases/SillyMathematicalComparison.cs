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

            const int veryBig = byte.MaxValue + 1;
            const int verySmall = byte.MinValue - 1;

            _ = b < veryBig; // Compliant, raised by CS0652
            _ = veryBig > b; // Compliant, raised by CS0652
            _ = b > verySmall; // Compliant, raised by CS0652
            _ = verySmall < b; // Compliant, raised by CS0652
        }

        public void Floats()
        {
            const float smallFloat = float.MinValue;
            const float bigFloat = float.MaxValue;

            const double smallDouble = float.MinValue;
            const double bigDouble = float.MaxValue;

            const double veryBig = double.MaxValue;
            const double verySmall = double.MinValue;

            float f = 42;

            _ = f <= bigFloat; // Noncompliant
            _ = f <= bigDouble; // Noncompliant
            _ = f >= smallFloat; // Noncompliant
            _ = f >= smallDouble; // Noncompliant

            _ = bigFloat >= f; // Noncompliant
            _ = bigDouble >= f; // Noncompliant
            _ = smallFloat <= f; // Noncompliant
            _ = smallDouble <= f; // Noncompliant

            _ = f >= bigFloat; // Compliant, not (always true) or (always false)
            _ = f >= bigDouble; // Compliant, not (always true) or (always false)
            _ = f <= smallFloat; // Compliant, not (always true) or (always false)
            _ = f <= smallDouble; // Compliant, not (always true) or (always false)

            _ = bigFloat <= f; // Compliant, not (always true) or (always false)
            _ = bigDouble <= f; // Compliant, not (always true) or (always false)
            _ = smallFloat >= f; // Compliant, not (always true) or (always false)
            _ = smallDouble >= f; // Compliant, not (always true) or (always false)

            _ = f < veryBig; // Noncompliant
            _ = veryBig > f; // Noncompliant
            _ = f > verySmall; // Noncompliant
            _ = verySmall < f; // Noncompliant
        }

    }
}
