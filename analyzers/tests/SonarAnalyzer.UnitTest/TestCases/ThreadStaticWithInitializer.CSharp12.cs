using System;
using Point2D = (int, int);

namespace Tests.Diagnostics
{
    public class ThreadStaticWithInitializer
    {
        public class Foo
        {
            [ThreadStatic]
            public static int[] PerThreadArray = [1, 2, 3]; // Noncompliant

            [ThreadStatic]
            public static Point2D PerThreadPoint = new Point2D(); // Noncompliant
        }
    }
}
