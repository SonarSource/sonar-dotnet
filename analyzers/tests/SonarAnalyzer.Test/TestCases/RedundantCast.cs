using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class RedundantCast
    {
        void foo(long l)
        {
            var x = new int[] { 1, 2, 3 }.Cast<int>(); // Noncompliant
//                                       ^^^^^^^^^^^^
            x = Enumerable // Noncompliant
                .Cast<int>(new int[] { 1, 2, 3 });
            x = x
                .OfType<int>(); //Noncompliant
            x = x.Cast<int>(); //Noncompliant

            var y = x.OfType<object>();

            var zz = (int)l;
            int i = 0;
            var z = (int)i; // Noncompliant {{Remove this unnecessary cast to 'int'.}}
//                   ^^^
            z = (Int32)i; // Noncompliant {{Remove this unnecessary cast to 'int'.}}

            var w = (object)i;

            method(new int[] { 1, 2, 3 }.Cast<int>()); // Noncompliant {{Remove this unnecessary cast to 'IEnumerable<int>'.}}
        }
        void method(IEnumerable<int> enumerable)
        { }

        void M()
        {
            var o = new object();
            var oo = o as object; // Noncompliant
            var i = o as RedundantCast; // Compliant
        }

        public void N(int[,] numbers)
        {
            numbers.Cast<int>().Where(x => x > 0); // Compliant, multidimensional arrays need to be cast
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/3273
        public void OfTypeWithReferenceTypes(IEnumerable<object> param)
        {
            var stringArrayWithNull = new string[] { "one", "two", null, "three" };
            var filteredStringArray = stringArrayWithNull.OfType<string>(); // Compliant, has 3 items, without 'null'

            var enumerableOfObjects = new object[] { 1, "one", null };
            var filteredObjectArray = enumerableOfObjects.OfType<object>(); // Compliant, has 2 items, without 'null'

            var enumerableOfNullableInt = new int?[] { 1, 2, null };
            var filteredInts = enumerableOfNullableInt.OfType<int?>(); // Compliant, has 2 items, without 'null'

            param.OfType<object>(); // Compliant, may contain null values

            var useless = new string[] { "one", "two" }.OfType<string>(); // FN
        }

        public void CastWithReferenceTypes(IEnumerable<object> param)
        {
            var stringArrayWithNull = new string[] { "one", "two", null, "three" };
            var castStringArray = stringArrayWithNull.Cast<string>(); // Noncompliant

            var enumerableOfObjects = new object[] { 1, "one", null };
            var filteredObjectArray = enumerableOfObjects.Cast<object>(); // Noncompliant

            var enumerableOfNullableInt = new int?[] { 1, 2, null };
            var filteredInts = enumerableOfNullableInt.Cast<int?>(); // Noncompliant

            param.Cast<object>(); // Noncompliant
        }
    }

    public static class MyEnumerableExtensions
    {
        public static IEnumerable<T1> OfType<T1, T2>(this IEnumerable source) =>
            source.OfType<T1>();
        public static IEnumerable<T1> Cast<T1, T2>(this IEnumerable source) =>
            source.Cast<T1>();
        public static int OfType<T1, T2, T3>(this IEnumerable source) => 0;
        public static int Cast<T1, T2, T3>(this IEnumerable source) => 0;
    }

    public class TestWithCustomExtension
    {
        public void UnlikelyCases(IEnumerable<int> intValues, List<string> stringValues)
        {
            intValues.OfType<int, string>(); // Noncompliant
            intValues.Cast<int, string>(); // Noncompliant
            stringValues.OfType<string, string>();
            stringValues.Cast<string, string>(); // Noncompliant

            intValues.OfType<int, int, int>();
            intValues.Cast<int, int, int>();
        }
    }

    public class MoreTests
    {
        public void Foo(IEnumerable enumerable, IEnumerable<object> genericEnumerable)
        {
            enumerable.OfType<int>();
            enumerable.OfType<string>();

            genericEnumerable.Select(x => x).Select(x => x).OfType<string>();
            genericEnumerable.Select(x => x).Select(x => x).OfType<string>().OfType<string>();
            genericEnumerable.Select(x => x).Select(x => x).OfType<int>();
            genericEnumerable.Select(x => x).Select(x => x).OfType<int>().OfType<int>(); // Noncompliant
        }

        public void OfType() => OfType();
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/9498
    public class Repro_9498
    {
        void Sample(float x)
        {
            float res = 1 / (float)(x * 2); // Noncompliant FP
        }
    }

    // https://sonarsource.atlassian.net/browse/NET-1183
    public class NumberLiteralSuffixes
    {
        void Numbers()
        {
            var d0 = (decimal)12.0;     // FN: use 12.0m
            var d1 = (decimal)12;       // FN: use 12m
            var d2 = (double)12;        // FN: use 12d
            var f0 = (float)12.0;       // FN: use 12.0f
            var n0 = (uint)12;          // FN: use 12u
            var n1 = (long)12;          // FN: use 12l
            var n2 = (ulong)12;         // FN: use 12ul

            var n3 = (uint)0x12;        // FN: use 0x12u
            var n4 = (uint)0b101;       // FN: use 0b101u

            var n5 = (UInt32)12;        // FN: use 12u
            var n6 = (byte)12;          // Compliant, there is no suffix for byte
            var n7 = 0xFF_FF_FF_FF_FFl; // Compliant, "l" is redundant here but for readability it's better to keep it
        }
    }
}
