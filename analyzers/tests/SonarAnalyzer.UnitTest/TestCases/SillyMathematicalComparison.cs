using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.UnitTest.TestCases
{
    internal class SillyMathematicalComparison
    {
        public void SBytes()
        {
            const sbyte smallSByte = sbyte.MinValue;
            const short smallShort = sbyte.MinValue;
            const int smallInt = sbyte.MinValue;
            const long smallLong = sbyte.MinValue;
            const float smallFloat = sbyte.MinValue;
            const double smallDouble = sbyte.MinValue;
            const decimal smallDecimal = sbyte.MinValue;

            const sbyte bigSByte = sbyte.MaxValue;
            const short bigShort = sbyte.MaxValue;
            const int bigInt = sbyte.MaxValue;
            const long bigLong = sbyte.MaxValue;
            const float bigFloat = sbyte.MaxValue;
            const double bigDouble = sbyte.MaxValue;
            const decimal bigDecimal = sbyte.MaxValue;

            sbyte sb = 42;

            _ = sb >= smallSByte; // Noncompliant
            _ = sb >= smallShort; // Noncompliant
            _ = sb >= smallInt; // Noncompliant
            _ = smallLong <= sb; // Noncompliant
            _ = smallFloat <= sb; // Noncompliant
            _ = smallDouble <= sb; // Noncompliant
            _ = smallDecimal <= sb; // Noncompliant

            _ = sb <= bigSByte; // Noncompliant
            _ = sb <= bigShort; // Noncompliant
            _ = sb <= bigInt; // Noncompliant
            _ = bigLong >= sb; // Noncompliant
            _ = bigFloat >= sb; // Noncompliant
            _ = bigDouble >= sb; // Noncompliant
            _ = bigDecimal >= sb; // Noncompliant

            _ = sb >= bigSByte; // Compliant, not (always true) or (always false)
            _ = sb >= bigShort; // Compliant, not (always true) or (always false)
            _ = sb >= bigInt; // Compliant, not (always true) or (always false)
            _ = bigLong <= sb; // Compliant, not (always true) or (always false)
            _ = bigFloat <= sb; // Compliant, not (always true) or (always false)
            _ = bigDouble <= sb; // Compliant, not (always true) or (always false)
            _ = bigDecimal <= sb; // Compliant, not (always true) or (always false)

            _ = sb <= smallSByte; // Compliant, not (always true) or (always false)
            _ = sb <= smallShort; // Compliant, not (always true) or (always false)
            _ = sb <= smallInt; // Compliant, not (always true) or (always false)
            _ = smallLong >= sb; // Compliant, not (always true) or (always false)
            _ = smallFloat >= sb; // Compliant, not (always true) or (always false)
            _ = smallDouble >= sb; // Compliant, not (always true) or (always false)
            _ = smallDecimal >= sb; // Compliant, not (always true) or (always false)

            const int veryBig = int.MaxValue;
            const long verySmall = long.MinValue;

            _ = sb < veryBig; // Compliant, raised by CS0652
            _ = veryBig > sb; // Compliant, raised by CS0652
            _ = sb > verySmall; // Compliant, raised by CS0652
            _ = verySmall < sb; // Compliant, raised by CS0652
        }

        public void Bytes()
        {
            const byte smallByte = byte.MinValue;
            const short smallShort = byte.MinValue;
            const ushort smallUShort = byte.MinValue;
            const int smallInt = byte.MinValue;
            const uint smallUInt = byte.MinValue;
            const long smallLong = byte.MinValue;
            const ulong smallULong = byte.MinValue;
            const float smallFloat = byte.MinValue;
            const double smallDouble = byte.MinValue;
            const decimal smallDecimal = byte.MinValue;

            const byte bigByte = byte.MaxValue;
            const short bigShort = byte.MaxValue;
            const ushort bigUShort = byte.MaxValue;
            const int bigInt = byte.MaxValue;
            const uint bigUInt = byte.MaxValue;
            const long bigLong = byte.MaxValue;
            const ulong bigULong = byte.MaxValue;
            const float bigFloat = byte.MaxValue;
            const double bigDouble = byte.MaxValue;
            const decimal bigDecimal = byte.MaxValue;

            byte b = 42;

            _ = b >= smallByte; // Noncompliant
            _ = b >= smallShort; // Noncompliant
            _ = b >= smallUShort; // Noncompliant
            _ = b >= smallInt; // Noncompliant
            _ = b >= smallUInt; // Noncompliant
            _ = smallLong <= b; // Noncompliant
            _ = smallULong <= b; // Noncompliant
            _ = smallFloat <= b; // Noncompliant
            _ = smallDouble <= b; // Noncompliant
            _ = smallDecimal <= b; // Noncompliant

            _ = b <= bigByte; // Noncompliant
            _ = b <= bigShort; // Noncompliant
            _ = b <= bigUShort; // Noncompliant
            _ = b <= bigInt; // Noncompliant
            _ = b <= bigUInt; // Noncompliant
            _ = bigLong >= b; // Noncompliant
            _ = bigULong >= b; // Noncompliant
            _ = bigFloat >= b; // Noncompliant
            _ = bigDouble >= b; // Noncompliant
            _ = bigDecimal >= b; // Noncompliant

            _ = b >= bigByte; // Compliant, not (always true) or (always false)
            _ = b >= bigShort; // Compliant, not (always true) or (always false)
            _ = b >= bigUShort; // Compliant, not (always true) or (always false)
            _ = b >= bigInt; // Compliant, not (always true) or (always false)
            _ = b >= bigUInt; // Compliant, not (always true) or (always false)
            _ = bigLong <= b; // Compliant, not (always true) or (always false)
            _ = bigULong <= b; // Compliant, not (always true) or (always false)
            _ = bigFloat <= b; // Compliant, not (always true) or (always false)
            _ = bigDouble <= b; // Compliant, not (always true) or (always false)
            _ = bigDecimal <= b; // Compliant, not (always true) or (always false)

            _ = b <= smallByte; // Compliant, not (always true) or (always false)
            _ = b <= smallShort; // Compliant, not (always true) or (always false)
            _ = b <= smallUShort; // Compliant, not (always true) or (always false)
            _ = b <= smallInt; // Compliant, not (always true) or (always false)
            _ = b <= smallUInt; // Compliant, not (always true) or (always false)
            _ = smallLong >= b; // Compliant, not (always true) or (always false)
            _ = smallULong >= b; // Compliant, not (always true) or (always false)
            _ = smallFloat >= b; // Compliant, not (always true) or (always false)
            _ = smallDouble >= b; // Compliant, not (always true) or (always false)
            _ = smallDecimal >= b; // Compliant, not (always true) or (always false)

            const int veryBig = int.MaxValue;
            const short verySmall = short.MinValue;

            _ = b < veryBig; // Compliant, raised by CS0652
            _ = veryBig > b; // Compliant, raised by CS0652
            _ = b > verySmall; // Compliant, raised by CS0652
            _ = verySmall < b; // Compliant, raised by CS0652
        }

        public void Shorts()
        {
            const short smallShort = short.MinValue;
            const int smallInt = short.MinValue;
            const long smallLong = short.MinValue;
            const float smallFloat = short.MinValue;
            const double smallDouble = short.MinValue;
            const decimal smallDecimal = short.MinValue;

            const short bigShort = short.MaxValue;
            const int bigInt = short.MaxValue;
            const long bigLong = short.MaxValue;
            const float bigFloat = short.MaxValue;
            const double bigDouble = short.MaxValue;
            const decimal bigDecimal = short.MaxValue;

            short s = 42;

            _ = s >= smallShort; // Noncompliant
            _ = s >= smallInt; // Noncompliant
            _ = smallLong <= s; // Noncompliant
            _ = smallFloat <= s; // Noncompliant
            _ = smallDouble <= s; // Noncompliant
            _ = smallDecimal <= s; // Noncompliant

            _ = s <= bigShort; // Noncompliant
            _ = s <= bigInt; // Noncompliant
            _ = bigLong >= s; // Noncompliant
            _ = bigFloat >= s; // Noncompliant
            _ = bigDouble >= s; // Noncompliant
            _ = bigDecimal >= s; // Noncompliant

            _ = s >= bigShort; // Compliant, not (always true) or (always false)
            _ = s >= bigInt; // Compliant, not (always true) or (always false)
            _ = bigLong <= s; // Compliant, not (always true) or (always false)
            _ = bigFloat <= s; // Compliant, not (always true) or (always false)
            _ = bigDouble <= s; // Compliant, not (always true) or (always false)
            _ = bigDecimal <= s; // Compliant, not (always true) or (always false)

            _ = s <= smallShort; // Compliant, not (always true) or (always false)
            _ = s <= smallInt; // Compliant, not (always true) or (always false)
            _ = smallLong >= s; // Compliant, not (always true) or (always false)
            _ = smallFloat >= s; // Compliant, not (always true) or (always false)
            _ = smallDouble >= s; // Compliant, not (always true) or (always false)
            _ = smallDecimal >= s; // Compliant, not (always true) or (always false)

            const int veryBig = int.MaxValue;
            const long verySmall = long.MinValue;

            _ = s < veryBig; // Compliant, raised by CS0652
            _ = veryBig > s; // Compliant, raised by CS0652
            _ = s > verySmall; // Compliant, raised by CS0652
            _ = verySmall < s; // Compliant, raised by CS0652
        }

        public void UShorts()
        {
            const ushort smallUShort = ushort.MinValue;
            const int smallInt = ushort.MinValue;
            const uint smallUInt = ushort.MinValue;
            const long smallLong = ushort.MinValue;
            const ulong smallULong = ushort.MinValue;
            const float smallFloat = ushort.MinValue;
            const double smallDouble = ushort.MinValue;
            const decimal smallDecimal = ushort.MinValue;

            const ushort bigUShort = ushort.MaxValue;
            const int bigInt = ushort.MaxValue;
            const uint bigUInt = ushort.MaxValue;
            const long bigLong = ushort.MaxValue;
            const ulong bigULong = ushort.MaxValue;
            const float bigFloat = ushort.MaxValue;
            const double bigDouble = ushort.MaxValue;
            const decimal bigDecimal = ushort.MaxValue;

            ushort us = 42;

            _ = us >= smallUShort; // Noncompliant
            _ = us >= smallInt; // Noncompliant
            _ = us >= smallUInt; // Noncompliant
            _ = smallLong <= us; // Noncompliant
            _ = smallULong <= us; // Noncompliant
            _ = smallFloat <= us; // Noncompliant
            _ = smallDouble <= us; // Noncompliant
            _ = smallDecimal <= us; // Noncompliant

            _ = us <= bigUShort; // Noncompliant
            _ = us <= bigInt; // Noncompliant
            _ = us <= bigUInt; // Noncompliant
            _ = bigLong >= us; // Noncompliant
            _ = bigULong >= us; // Noncompliant
            _ = bigFloat >= us; // Noncompliant
            _ = bigDouble >= us; // Noncompliant
            _ = bigDecimal >= us; // Noncompliant

            _ = us >= bigUShort; // Compliant, not (always true) or (always false)
            _ = us >= bigInt; // Compliant, not (always true) or (always false)
            _ = us >= bigUInt; // Compliant, not (always true) or (always false)
            _ = bigLong <= us; // Compliant, not (always true) or (always false)
            _ = bigULong <= us; // Compliant, not (always true) or (always false)
            _ = bigFloat <= us; // Compliant, not (always true) or (always false)
            _ = bigDouble <= us; // Compliant, not (always true) or (always false)
            _ = bigDecimal <= us; // Compliant, not (always true) or (always false)

            _ = us <= smallUShort; // Compliant, not (always true) or (always false)
            _ = us <= smallInt; // Compliant, not (always true) or (always false)
            _ = us <= smallUInt; // Compliant, not (always true) or (always false)
            _ = smallLong >= us; // Compliant, not (always true) or (always false)
            _ = smallULong >= us; // Compliant, not (always true) or (always false)
            _ = smallFloat >= us; // Compliant, not (always true) or (always false)
            _ = smallDouble >= us; // Compliant, not (always true) or (always false)
            _ = smallDecimal >= us; // Compliant, not (always true) or (always false)

            const int veryBig = int.MaxValue;
            const long verySmall = long.MinValue;

            _ = us < veryBig; // Compliant, raised by CS0652
            _ = veryBig > us; // Compliant, raised by CS0652
            _ = us > verySmall; // Compliant, raised by CS0652
            _ = verySmall < us; // Compliant, raised by CS0652
        }

        public void Ints()
        {
            const int smallInt = int.MinValue;
            const long smallLong = int.MinValue;
            const float smallFloat = int.MinValue;
            const double smallDouble = int.MinValue;
            const decimal smallDecimal = int.MinValue;

            const int bigInt = int.MaxValue;
            const uint bigUInt = int.MaxValue;
            const long bigLong = int.MaxValue;
            const float bigFloat = int.MaxValue;
            const double bigDouble = int.MaxValue;
            const decimal bigDecimal = int.MaxValue;

            int i = 42;

            _ = i >= smallInt; // Noncompliant
            _ = smallLong <= i; // Noncompliant
            _ = smallFloat <= i; // Noncompliant
            _ = smallDouble <= i; // Noncompliant
            _ = smallDecimal <= i; // Noncompliant

            _ = i <= bigInt; // Noncompliant
            _ = i <= bigUInt; // Noncompliant
            _ = bigLong >= i; // Noncompliant
            _ = bigFloat >= i; // Compliant, "bigFloat" has lost precision here and is not int.MaxValue
            _ = bigDouble >= i; // Noncompliant
            _ = bigDecimal >= i; // Noncompliant

            _ = i >= bigInt; // Compliant, not (always true) or (always false)
            _ = i >= bigUInt; // Compliant, not (always true) or (always false)
            _ = bigLong <= i; // Compliant, not (always true) or (always false)
            _ = bigFloat <= i; // Compliant, not (always true) or (always false)
            _ = bigDouble <= i; // Compliant, not (always true) or (always false)
            _ = bigDecimal <= i; // Compliant, not (always true) or (always false)

            _ = i <= smallInt; // Compliant, not (always true) or (always false)
            _ = smallLong >= i; // Compliant, not (always true) or (always false)
            _ = smallFloat >= i; // Compliant, not (always true) or (always false)
            _ = smallDouble >= i; // Compliant, not (always true) or (always false)
            _ = smallDecimal >= i; // Compliant, not (always true) or (always false)

            const long veryBig = long.MaxValue;
            const long verySmall = long.MinValue;

            _ = i < veryBig; // Compliant, raised by CS0652
            _ = veryBig > i; // Compliant, raised by CS0652
            _ = i > verySmall; // Compliant, raised by CS0652
            _ = verySmall < i; // Compliant, raised by CS0652
        }

        public void UInts()
        {
            const uint smallUInt = uint.MinValue;
            const long smallLong = uint.MinValue;
            const ulong smallULong = uint.MinValue;
            const float smallFloat = uint.MinValue;
            const double smallDouble = uint.MinValue;
            const decimal smallDecimal = uint.MinValue;

            const uint bigUInt = uint.MaxValue;
            const long bigLong = uint.MaxValue;
            const ulong bigULong = uint.MaxValue;
            const float bigFloat = uint.MaxValue;
            const double bigDouble = uint.MaxValue;
            const decimal bigDecimal = uint.MaxValue;

            uint ui = 42;

            _ = ui >= smallUInt; // Noncompliant
            _ = smallLong <= ui; // Noncompliant
            _ = smallULong <= ui; // Noncompliant
            _ = smallFloat <= ui; // Noncompliant
            _ = smallDouble <= ui; // Noncompliant
            _ = smallDecimal <= ui; // Noncompliant

            _ = ui <= bigUInt; // Noncompliant
            _ = bigLong >= ui; // Noncompliant
            _ = bigULong >= ui; // Noncompliant
            _ = bigFloat >= ui; // Compliant, "bigFloat" has lost precision here and is not uint.MaxValue
            _ = bigDouble >= ui; // Noncompliant
            _ = bigDecimal >= ui; // Noncompliant

            _ = ui >= bigUInt; // Compliant, not (always true) or (always false)
            _ = bigLong <= ui; // Compliant, not (always true) or (always false)
            _ = bigULong <= ui; // Compliant, not (always true) or (always false)
            _ = bigFloat <= ui; // Compliant, not (always true) or (always false)
            _ = bigDouble <= ui; // Compliant, not (always true) or (always false)
            _ = bigDecimal <= ui; // Compliant, not (always true) or (always false)

            _ = ui <= smallUInt; // Compliant, not (always true) or (always false)
            _ = smallLong >= ui; // Compliant, not (always true) or (always false)
            _ = smallULong >= ui; // Compliant, not (always true) or (always false)
            _ = smallFloat >= ui; // Compliant, not (always true) or (always false)
            _ = smallDouble >= ui; // Compliant, not (always true) or (always false)
            _ = smallDecimal >= ui; // Compliant, not (always true) or (always false)

            const long veryBig = long.MaxValue;
            const long verySmall = long.MinValue;

            _ = ui < veryBig; // Compliant, raised by CS0652
            _ = veryBig > ui; // Compliant, raised by CS0652
            _ = ui > verySmall; // Compliant, raised by CS0652
            _ = verySmall < ui; // Compliant, raised by CS0652
        }

        public void Longs()
        {
            const long smallLong = long.MinValue;
            const float smallFloat = long.MinValue;
            const double smallDouble = long.MinValue;
            const decimal smallDecimal = long.MinValue;

            const long bigLong = long.MaxValue;
            const float bigFloat = long.MaxValue;
            const double bigDouble = long.MaxValue;
            const decimal bigDecimal = long.MaxValue;

            long l = 42;

            _ = smallLong <= l; // Noncompliant
            _ = smallFloat <= l; // Noncompliant
            _ = smallDouble <= l; // Noncompliant
            _ = smallDecimal <= l; // Noncompliant

            _ = bigLong >= l; // Noncompliant
            _ = bigFloat >= l; // Noncompliant
            _ = bigDouble >= l; // Noncompliant
            _ = bigDecimal >= l; // Noncompliant

            _ = bigLong <= l; // Compliant, not (always true) or (always false)
            _ = bigFloat <= l; // Compliant, not (always true) or (always false)
            _ = bigDouble <= l; // Compliant, not (always true) or (always false)
            _ = bigDecimal <= l; // Compliant, not (always true) or (always false)

            _ = smallLong >= l; // Compliant, not (always true) or (always false)
            _ = smallFloat >= l; // Compliant, not (always true) or (always false)
            _ = smallDouble >= l; // Compliant, not (always true) or (always false)
            _ = smallDecimal >= l; // Compliant, not (always true) or (always false)

            const double veryBig = double.MaxValue;
            const double verySmall = double.MinValue;

            _ = l < veryBig; // Noncompliant
            _ = veryBig > l; // Noncompliant
            _ = l > verySmall; // Noncompliant
            _ = verySmall < l; // Noncompliant
        }

        public void ULongs()
        {
            const ulong smallULong = ulong.MinValue;
            const float smallFloat = ulong.MinValue;
            const double smallDouble = ulong.MinValue;
            const decimal smallDecimal = ulong.MinValue;

            const ulong bigULong = ulong.MaxValue;
            const float bigFloat = ulong.MaxValue;
            const double bigDouble = ulong.MaxValue;
            const decimal bigDecimal = ulong.MaxValue;

            ulong ul = 42;

            _ = smallULong <= ul; // Noncompliant
            _ = smallFloat <= ul; // Noncompliant
            _ = smallDouble <= ul; // Noncompliant
            _ = smallDecimal <= ul; // Noncompliant

            _ = bigULong >= ul; // Noncompliant
            _ = bigFloat >= ul; // Noncompliant
            _ = bigDouble >= ul; // Noncompliant
            _ = bigDecimal >= ul; // Noncompliant

            _ = bigULong <= ul; // Compliant, not (always true) or (always false)
            _ = bigFloat <= ul; // Compliant, not (always true) or (always false)
            _ = bigDouble <= ul; // Compliant, not (always true) or (always false)
            _ = bigDecimal <= ul; // Compliant, not (always true) or (always false)

            _ = smallULong >= ul; // Compliant, not (always true) or (always false)
            _ = smallFloat >= ul; // Compliant, not (always true) or (always false)
            _ = smallDouble >= ul; // Compliant, not (always true) or (always false)
            _ = smallDecimal >= ul; // Compliant, not (always true) or (always false)

            const float veryBig = float.MaxValue;
            const float verySmall = float.MinValue;

            _ = ul < veryBig; // Noncompliant
            _ = veryBig > ul; // Noncompliant
            _ = ul > verySmall; // Noncompliant
            _ = verySmall < ul; // Noncompliant
        }

        public void Floats()
        {
            const float smallFloat = float.MinValue;
            const double smallDouble = float.MinValue;

            const float bigFloat = float.MaxValue;
            const double bigDouble = float.MaxValue;

            float f = 42;

            _ = smallFloat <= f; // Noncompliant
            _ = smallDouble <= f; // Noncompliant

            _ = bigFloat >= f; // Noncompliant
            _ = bigDouble >= f; // Noncompliant

            _ = bigFloat <= f; // Compliant, not (always true) or (always false)
            _ = bigDouble <= f; // Compliant, not (always true) or (always false)

            _ = smallFloat >= f; // Compliant, not (always true) or (always false)
            _ = smallDouble >= f; // Compliant, not (always true) or (always false)

            const double veryBig = double.MaxValue;
            const double verySmall = double.MinValue;

            _ = f < veryBig; // Noncompliant
            _ = veryBig > f; // Noncompliant
            _ = f > verySmall; // Noncompliant
            _ = verySmall < f; // Noncompliant
        }
    }
}
