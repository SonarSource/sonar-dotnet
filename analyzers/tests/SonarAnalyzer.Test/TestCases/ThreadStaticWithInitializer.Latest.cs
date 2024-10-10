using System;
using Point2D = (int, int);

namespace CSharp9
{
    record Foo
    {
        [ThreadStatic]
        public static object PerThreadObject = new object(); // Noncompliant {{Remove this initialization of 'PerThreadObject' or make it lazy.}}
//                                           ^^^^^^^^^^^^^^

        [ThreadStatic]
        public static object Compliant;
    }
}

namespace CSharp11
{
    public class ThreadStaticWithInitializer
    {
        public class Foo
        {
            [ThreadStaticAttribute<int>]
            public static object PerThreadObject1 = new object();   // FN: for performance reasons we decided not to handle derived classes

            [ThreadStaticAttribute]
            public static object PerThreadObject2 = new object();   // Noncompliant
        }

        public class ThreadStaticAttribute<T> : ThreadStaticAttribute { }
    }
}

namespace CSharp12
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

namespace CSharp13
{
    public partial class Partial
    {
        public static partial object PerThreadObject
        {
            get
            {
                if (_perThreadObject == null)
                {
                    _perThreadObject = new object();
                }
                return _perThreadObject;
            }
        }
    }

    public partial class Partial
    {
        [ThreadStatic]
        public static object _perThreadObject; // Compliant
        public static partial object PerThreadObject { get; }
    }
}
