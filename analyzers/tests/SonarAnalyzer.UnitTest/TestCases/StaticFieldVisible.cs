using System;

namespace Tests.TestCases
{
    public class StaticFieldVisible
    {
        public static double Pi = 3.14;  // Noncompliant
//                           ^^
        public static int X = 1, Y, Z = 100;
//                        ^
//                               ^@-1
//                                  ^@-2
        public const double Pi2 = 3.14;
        public double Pi3 = 3.14;

        protected static double Pi4 = 3.14; // Noncompliant
        internal static double Pi5 = 3.14; // Noncompliant
        internal static double Pi6 = 3.14; // Noncompliant
        protected internal static double Pi7 = 3.14; // Noncompliant

        private static double Pi8 = 3.14;
        private double Pi9 = 3.14;
        static double Pi10 = 3.14; // Noncompliant "internal" is the default if no access modifier is specified
                                   // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers

        [ThreadStatic]
        public static int value; // Compliant, thread static field values are not shared between threads
    }

    public class Shape
    {
        public static Shape Empty = new EmptyShape(); // Noncompliant {{Change the visibility of 'Empty' or make it 'const' or 'readonly'.}}
        public static readonly Shape Empty2 = new EmptyShape();

        private class EmptyShape : Shape
        {
        }
    }
}
