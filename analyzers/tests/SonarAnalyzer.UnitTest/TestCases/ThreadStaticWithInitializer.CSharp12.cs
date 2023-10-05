using System;
using Point2D = (int, int);

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
