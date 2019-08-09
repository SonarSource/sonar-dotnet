using System;

namespace Tests.Diagnostics
{
    public class ValidUseCases
    {
        private const int MAGIC = 42;
        public readonly int MY_VALUE = 25;

        public int A { get; set; } = 2;

        public static double FooProp
        {
            get { return 2; }
        }

        public ValidUseCases()
        {
            int i1 = 0;
            int i2 = -1;
            int i3 = 1;
            int i4 = 42;

            for (int i = 0; i < 0; i++)
            {

            }

            const int VAL = 15;

            Console.Write("test");
        }

        public override int GetHashCode()
        {
            return MY_VALUE * 397;
        }

        public void Foo(int value = 42)
        {
            var x = -1 < 1;
        }
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

        public WrongUseCases()
        {
            Console.WriteLine(12); // Noncompliant

            for (int i = 10; i < 50; i++) // Noncompliant
            {

            }

            var array = new string[10];
            array[5] = "test"; // Noncompliant
        }

        public int GetValue()
        {
            return 13; // Noncompliant
        }
    }
}

#pragma warning disable 1998 //async method lacks an await
