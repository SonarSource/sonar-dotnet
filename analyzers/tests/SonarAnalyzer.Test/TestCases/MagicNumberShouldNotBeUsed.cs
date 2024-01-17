using System;
using System.Collections.Generic;
using System.Linq;

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
        public FooBar fooBar { get; set; }
        public FooBar GetFooBar() => null;
        public int Count() => 0;
        public FooBar Length() => null; // for coverage
    }

    public class Baz
    {
        public int Size() => 1;
    }

    public class ValidUseCases
    {
        private const int MAGIC = 42;
        private IntPtr Pointer = new IntPtr(19922); // Compliant, store in field
        public readonly int MY_VALUE = 25;
        const int ONE_YEAR_IN_SECONDS = 31_557_600;
        static readonly int ReadOnlyValue = Bar(999); // Compliant as stored in static readonly

        public int A { get; set; } = 2;

        public static double FooProp
        {
            get { return 2; }
        }

        public static double FooProp2
        {
            get
            {
                var someName = Math.Sqrt(4096); // Compliant, stored in a variable
                return someName;
            }
        }

        public ValidUseCases(int x, int y) { }

        public ValidUseCases(string s, FooBar foo, Baz baz)
        {
            int i1 = 0;
            int i2 = -1;
            int i3 = 1;
            int i4 = 42;
            var list = new List<string>(42); // Compliant, set to variable
            var bigInteger = new IntPtr(1612342); // compliant, set to variable
            var g = new Guid(0xA, 0xB, 0xC, new Byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }); // Compliant, set to a variable
            var valid = new ValidUseCases(100, 300);
            var fooResult = Bar(42, Bar(110, Bar(233, 23454, Bar(999)))); // FN https://github.com/SonarSource/sonar-dotnet/issues/5251
            Foo(new IntPtr(123456)); // compliant, constructor

            long[] values1 = { 30L, 40L }; // Compliant, set to variable
            int[] values2 = new int[] { 100, 200 }; // Compliant, set to variable
            Console.WriteLine(value: 12); // Compliant, named argument

            for (int i = 0; i < 0; i++) { }

            var result = list.Count == 1; // Compliant, single digit for Count
            result = list.Count < 2; // Compliant
            result = list.Count <= 2; // Compliant
            result = 2 <= list.Count; // Compliant
            result = list.Count != 2; // Compliant
            result = list.Count > 2; // Compliant
            result = list.Count() > 2; // Compliant
            result = s.Length == 3; // Compliant, single digit for Length
            result = foo.Size == 4; // Compliant, single digit for Size
            result = baz.Size() == 7; // tolerated FN
            result = foo.fooBar.fooBar.fooBar.Size == 4; // Compliant
            result = foo.GetFooBar().fooBar.fooBar.GetFooBar().Count() == 4; // possible FN, we don't check if Count() is from LINQ to be efficient

            WithTimeSpan(TimeSpan.FromDays(1), TimeSpan.FromMilliseconds(33)); // compliant

            const int VAL = 15;

            Console.Write("test");
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

        public static void Foo(IntPtr x) { }

        [Foo(42)] // Compliant, attribute with only one argument
        public static int Bar(int value, params int[] values) => 0;

        public static void WithTimeSpan(TimeSpan one, TimeSpan two) { }
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
            get { return Math.Sqrt(4); } // Noncompliant {{Assign this magic number '4' to a well-named variable or constant, and use that instead.}}
//                                 ^
        }

        public WrongUseCases(int x, int y) { }

        public WrongUseCases(List<int> list, string s, FooBar foo)
        {
            Console.WriteLine(12); // Noncompliant {{Assign this magic number '12' to a well-named variable or constant, and use that instead.}}

            for (int i = 10; i < 50; i++) // Noncompliant {{Assign this magic number '50' to a well-named variable or constant, and use that instead.}}
            {

            }

            var array = new string[10];
            array[5] = "test"; // Noncompliant {{Assign this magic number '5' to a well-named variable or constant, and use that instead.}}
            Foo(new int[] { 100 }); // Noncompliant

            new WrongUseCases(100, Foo(200, 300)); // Noncompliant {{Assign this magic number '200' to a well-named variable or constant, and use that instead.}}
            // Noncompliant@-1 {{Assign this magic number '300' to a well-named variable or constant, and use that instead.}}

            Foo(new int[] { 100, 200 }); // Noncompliant {{Assign this magic number '100' to a well-named variable or constant, and use that instead.}}
            // Noncompliant@-1 {{Assign this magic number '200' to a well-named variable or constant, and use that instead.}}

            GetSomeFrom(42); // Noncompliant

            for (int i = 0; i < list.Count / 8; i++) { } // Noncompliant

            var result = list.Count == 99;
            result = list.Count < 400; // Noncompliant
            result = s.Length == 121; // Noncompliant
            result = foo.Size == 472; // Noncompliant
            result = foo.baz == 4; // Noncompliant
            result = s.Length == list.Count / 2 ; // Noncompliant FP - clear it's checking if it's half the list

            result = foo.COUNT == 8; // Noncompliant
            result = foo.length == 9; // Noncompliant
            result = GetCount() < 9; // Noncompliant
            result = SizeOfSomething == 5; // Noncompliant
            // for coverage
            result = foo.Length().fooBar.length == 4; // Noncompliant

            var x = 1;
            result = x == 2; // Noncompliant
            Foo(foo.Size + 41, s.Length - 43, list.Count * 2, list.Count / 8); // Noncompliant
            // Noncompliant@-1
            // Noncompliant@-2
            // Noncompliant@-3
        }

        [Foo(42, 43)] // Noncompliant
        // Noncompliant@-1
        public int GetValue()
        {
            return 13; // Noncompliant
        }

        public int GetCount() => 1;
        public int SizeOfSomething { get; } = 1;

        public static void Foo(params int[] array) { }
        public static int Foo(int x, int y) => 1;
        public static void GetSomeFrom(int value) { }
    }
}

#pragma warning disable 1998 //async method lacks an await
