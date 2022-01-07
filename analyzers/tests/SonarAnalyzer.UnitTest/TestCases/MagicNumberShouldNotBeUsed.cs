using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class FooAttribute : Attribute
    {
        public int Baz { get; set; }
        public FooAttribute(int Bar = 0, int Bar2 = 0)
        {
        }
    }

    public class FooBar
    {
        public int Size { get; set; }
        public int COUNT { get; set; }
        public int length { get; set; }
        public int baz { get; set; }
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

        public ValidUseCases(int x, int y) { }

        public ValidUseCases(string s, FooBar foo)
        {
            int i1 = 0;
            int i2 = -1;
            int i3 = 1;
            int i4 = 42;
            var list = new List<string>(42); // Compliant, set to variable
            var bigInteger = new IntPtr(1612342); // compliant, set to variable
            var g = new Guid(0xA, 0xB, 0xC, new Byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }); // Compliant, set in a variable

            Foo(new IntPtr(123456)); // Compliant, class constructor
            var byteArrayName = new Byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            Foo(new Guid(0xA, 0xB, 0xC, byteArrayName)); // Compliant, magic numbers set directly as constructor params

            long[] values1 = { 30L, 40L }; // Compliant, array initialisation
            int[] values2 = new int[] { 100, 200 }; // Compliant, array initialisation
            Console.WriteLine(value: 12); // Compliant, named argument

            for (int i = 0; i < 0; i++)
            {

            }

            var result = list.Count == 1; // Compliant, single digit for Count
            result = list.Count < 2; // Compliant, single digit for Count
            result = list.Count <= 2; // Compliant
            result = list.Count != 2; // Compliant
            result = list.Count > 2; // Compliant
            result = s.Length == 3; // Compliant, single digit for Length
            result = foo.Size == 4; // Compliant, single digit for Size
            result = foo.COUNT == 8; // Compliant
            result = foo.length == 9; // Compliant

            const int VAL = 15;

            Console.Write("test");

            // we tolerate magic constants sent to constructors
            new ValidUseCases(100, 300);
        }

        public override int GetHashCode()
        {
            return MY_VALUE * 397;
        }

        [Foo(Bar: 42)] // Compliant, explicit attribute argument names
        public void Foo1() { }

        [Foo(Baz = 43)] // Compliant, explicit attribute argument names
        public void Foo2() { }

        [Foo(Bar: 42, Baz = 43)] // Compliant, explicit attribute argument names
        public void Foo3() { }

        public void Foo(int value = 42) // compliant, default value for argument
        {
            var x = -1 < 1;
        }

        [Foo(42)] // Compliant, attribute with only one argument
        public static int Bar(int value) => 0;

        public static void Foo(IntPtr x) { }
        public static void Foo(Guid x) { }
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

        public WrongUseCases(int x, int y) { }

        public WrongUseCases(List<int> list, string s, FooBar foo)
        {
            Console.WriteLine(12); // Noncompliant

            for (int i = 10; i < 50; i++) // Noncompliant
            {

            }

            var array = new string[10];
            array[5] = "test"; // Noncompliant
            Foo(new int[] { 100 }); // Noncompliant, array with magic numbers should have a decent name


            new WrongUseCases(100, Foo(200, 300)); // Noncompliant {{Assign this magic number '200' to a well-named (variable|constant), and use the (variable|constant) instead.}}
            // Noncompliant@-1 {{Assign this magic number '300' to a well-named (variable|constant), and use the (variable|constant) instead.}}

            var result = list.Count == 99;
            result = list.Count < 400; // Noncompliant
            result = s.Length == 121; // Noncompliant
            result = foo.Size == 472; // Noncompliant
            result = foo.baz == 4; // Noncompliant
            var x = 1;
            result = x == 2; // Noncompliant
        }

        [Foo(42, 43)] // Noncompliant
        // Noncompliant@-1
        public int GetValue()
        {
            return 13; // Noncompliant
        }

        public static void Foo(int[] array) { }
        public static int Foo(int x, int y) => 1;
    }
}

#pragma warning disable 1998 //async method lacks an await
