using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class FooAttribute : Attribute
    {
        public int Baz { get; set; }
        public FooAttribute(int Bar, int Bar2 = 0)
        {
        }
    }

    public class FooBar
    {
        public int Size { get; set; }
    }

    public class ValidUseCases
    {
        private const int MAGIC = 42;
        private IntPtr Pointer = new IntPtr(19922); // Compliant, class constructor
        public readonly int MY_VALUE = 25;
        const int ONE_YEAR_IN_SECONDS = 31_557_600;
        static readonly int ReadOnlyValue = Bar(999); // Compliant as stored in static readonly

        public int A { get; set; } = 2;

        public static double FooProp
        {
            get { return 2; }
        }

        public ValidUseCases(string s, FooBar foo)
        {
            int i1 = 0;
            int i2 = -1;
            int i3 = 1;
            int i4 = 42;
            var list = new List<string>(42); // Compliant, class constructor
            var bigInteger = new IntPtr(1612342); // Compliant, class constructor
            var g = new Guid(0xA, 0xB, 0xC, new Byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }); // Compliant, class constructor
            long[] values1 = { 30L, 40L }; // Compliant, array initialisation
            int[] values2 = new int[] { 100, 200 }; // Compliant, array initialisation
            Console.WriteLine(value: 12); // Compliant, named argument

            for (int i = 0; i < 0; i++)
            {

            }

            var result = list.Count == 9; // Compliant, single digit for Count
            result = list.Count < 4; // Noncompliant FP - Compliant, single digit for Count
            result = s.Length == 2; // Noncompliant FP - Compliant, single digit for Length
            result = foo.Size == 2; // Noncompliant FP - Compliant, single digit for Size

            const int VAL = 15;

            Console.Write("test");
        }

        public override int GetHashCode()
        {
            return MY_VALUE * 397;
        }

        [Foo(Bar: 42, Baz = 43)] // Noncompliant FP - Compliant, explicit attribute argument names
        // Noncompliant @-1
        public void Foo(int value = 42)
        {
            var x = -1 < 1;
        }

        [Foo(42)] // Noncompliant FP -Compliant, attribute with only one argument
        public static int Bar(int value) => 0;
    }

    public enum MyEnum
    {
        Value1 = 1,
        Value2 = 2,
        Value3 = 3
    }

    public class WrongUseCases
    {
        public static double FooProp
        {
            get { return Math.Sqrt(4); } // Noncompliant
        }

        public WrongUseCases(List<int> list, string s, FooBar foo)
        {
            Console.WriteLine(12); // Noncompliant

            for (int i = 10; i < 50; i++) // Noncompliant
            {

            }

            var array = new string[10];
            array[5] = "test"; // Noncompliant

            var result = list.Count == 99;
            result = list.Count < 400; // Noncompliant
            result = s.Length == 121; // Noncompliant
            result = foo.Size == 472; // Noncompliant
        }

        [Foo(42, 43)] // Noncompliant
        // Noncompliant@-1
        public int GetValue()
        {
            return 13; // Noncompliant
        }
    }
}

#pragma warning disable 1998 //async method lacks an await
