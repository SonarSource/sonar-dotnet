using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.Test.TestCases
{
    internal class UnnecessaryMathematicalComparison
    {
        private T GetValue<T>() => default(T);

        public void Chars() // Rule applies
        {
            const char smallChar = char.MinValue;
            const int smallInt = char.MinValue;
            const long smallLong = char.MinValue;
            const float smallFloat = char.MinValue;
            const double smallDouble = char.MinValue;
            const decimal smallDecimal = char.MinValue;

            const char bigChar = char.MaxValue;
            const int bigInt = char.MaxValue;
            const long bigLong = char.MaxValue;
            const float bigFloat = char.MaxValue;
            const double bigDouble = char.MaxValue;
            const decimal bigDecimal = char.MaxValue;

            var c = GetValue<char>();

            _ = c >= bigChar; // Compliant, not (always true) or (always false)
            _ = c >= bigInt; // Compliant, not (always true) or (always false)
            _ = bigLong <= c; // Compliant, not (always true) or (always false)
            _ = bigFloat <= c; // Compliant, not (always true) or (always false)
            _ = bigDouble <= c; // Compliant, not (always true) or (always false)
            _ = bigDecimal <= c; // Compliant, not (always true) or (always false)

            _ = c <= smallChar; // Compliant, not (always true) or (always false)
            _ = c <= smallInt; // Compliant, not (always true) or (always false)
            _ = smallLong >= c; // Compliant, not (always true) or (always false)
            _ = smallFloat >= c; // Compliant, not (always true) or (always false)
            _ = smallDouble >= c; // Compliant, not (always true) or (always false)
            _ = smallDecimal >= c; // Compliant, not (always true) or (always false)

            const int veryBig = int.MaxValue;
            const long verySmall = long.MinValue;

            _ = c < veryBig; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'char'}}
            _ = veryBig > c; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'char'}}
            _ = c > verySmall; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'char'}}
            _ = verySmall < c; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'char'}}
            _ = verySmall == c; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'char'}}
            _ = c == veryBig; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'char'}}
        }

        public void SBytes() // CS0652
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

            var sb = GetValue<sbyte>();

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
            _ = verySmall == sb; // Compliant, raised by CS0652
            _ = sb == veryBig; // Compliant, raised by CS0652
        }

        public void Bytes() // CS0652
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

            var b = GetValue<byte>();

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
            _ = verySmall == b; // Compliant, raised by CS0652
            _ = b == veryBig; // Compliant, raised by CS0652
        }

        public void Shorts() // CS0652
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

            var s = GetValue<short>();

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
            _ = verySmall == s; // Compliant, raised by CS0652
            _ = s == veryBig; // Compliant, raised by CS0652
        }

        public void UShorts() // CS0652
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

            var us = GetValue<ushort>();

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
            _ = verySmall == us; // Compliant, raised by CS0652
            _ = us == veryBig; // Compliant, raised by CS0652
        }

        public void Ints() // CS0652
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

            var i = GetValue<int>();

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
            _ = verySmall == i; // Compliant, raised by CS0652
            _ = i == veryBig; // Compliant, raised by CS0652
        }

        public void UInts() // CS0652
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

            var ui = GetValue<uint>();

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
            _ = verySmall == ui; // Compliant, raised by CS0652
            _ = ui == veryBig; // Compliant, raised by CS0652
        }

        public void Longs() // Rule applies
        {
            const long smallLong = long.MinValue;
            const float smallFloat = long.MinValue;
            const double smallDouble = long.MinValue;
            const decimal smallDecimal = long.MinValue;

            const long bigLong = long.MaxValue;
            const float bigFloat = long.MaxValue;
            const double bigDouble = long.MaxValue;
            const decimal bigDecimal = long.MaxValue;

            var l = GetValue<long>();

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

            _ = l < veryBig; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'long'}}
            _ = veryBig > l; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'long'}}
            _ = l > verySmall; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'long'}}
            _ = verySmall < l; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'long'}}
            _ = verySmall == l; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'long'}}
            _ = l == veryBig; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'long'}}
        }

        public void ULongs() // Rule applies
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

            _ = ul < veryBig; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'ulong'}}
            _ = veryBig > ul; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'ulong'}}
            _ = ul > verySmall; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'ulong'}}
            _ = verySmall < ul; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'ulong'}}
            _ = verySmall == ul; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'ulong'}}
            _ = ul == veryBig; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'ulong'}}
        }

        public void Floats() // Rule applies
        {
            const float smallFloat = float.MinValue;
            const double smallDouble = float.MinValue;

            const float bigFloat = float.MaxValue;
            const double bigDouble = float.MaxValue;


            const long l = long.MaxValue;
            var i = 42;

            System.Single f = GetValue<float>();

            _ = bigFloat <= f; // Compliant, not (always true) or (always false)
            _ = bigDouble <= f; // Compliant, not (always true) or (always false)

            _ = smallFloat >= f; // Compliant, not (always true) or (always false)
            _ = smallDouble >= f; // Compliant, not (always true) or (always false)

            const double veryBig = double.MaxValue;
            const double verySmall = double.MinValue;

            _ = f < veryBig; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'float'}}
            _ = veryBig > f; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'float'}}
            _ = f > verySmall; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'float'}}
            _ = verySmall < f; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'float'}}
            _ = verySmall == f; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'float'}}
            _ = f == veryBig; // Noncompliant {{Comparison to this constant is useless; the constant is outside the range of type 'float'}}
        }

        public void ConstantInBothOperands()
        {
            const float number1 = 42;
            const double number2 = double.MaxValue;

            // https://github.com/SonarSource/sonar-dotnet/issues/6745
            _ = number1 != number2; // Compliant, constant in both operands
            _ = number1 == number2; // Compliant
            _ = number1 <= double.MaxValue; // Compliant
            _ = double.MaxValue >= number2; // Compliant

            _ = 42f != double.MaxValue; // Compliant
            _ = 42f <= double.MaxValue; // Compliant
        }

        public void Edgecases()
        {
            const float fPositiveInfinity = float.PositiveInfinity;
            const float fNegativeInfinity = float.NegativeInfinity;
            const double dPositiveInfinity = double.PositiveInfinity;
            const double dNegativeInfinity = double.NegativeInfinity;

            const float fNan = float.NaN;
            const double dNan = double.NaN;

            float f = 42;

            _ = f <= fPositiveInfinity; // Compliant
            _ = fNegativeInfinity < f; // Compliant

            _ = fNegativeInfinity != f; // Compliant
            _ = dNegativeInfinity == f; // Compliant

            _ = f >= dPositiveInfinity; // Compliant
            _ = dNegativeInfinity > f; // Compliant
            _ = f != fPositiveInfinity; // Compliant
            _ = f == dPositiveInfinity; // Compliant


            _ = f == fNan; // Compliant
            _ = f != fNan; // Compliant
            _ = f <= fNan; // Compliant
            _ = f >= fNan; // Compliant
            _ = f > fNan; // Compliant
            _ = f < fNan; // Compliant

            _ = dNan == f; // Compliant
            _ = dNan != f; // Compliant
            _ = dNan <= f; // Compliant
            _ = dNan >= f; // Compliant
            _ = dNan > f; // Compliant
            _ = dNan < f; // Compliant
        }
    }
}
