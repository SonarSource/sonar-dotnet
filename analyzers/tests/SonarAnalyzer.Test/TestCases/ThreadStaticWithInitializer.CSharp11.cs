using System;

namespace Tests.Diagnostics
{
    public class ThreadStaticWithInitializer
    {
        public class Foo
        {
            [ThreadStaticAttribute<int>]
            public static object PerThreadObject1 = new object();   // FN for performance reasons we decided not to handle derived classes

            [ThreadStaticAttribute]
            public static object PerThreadObject2 = new object();   // Noncompliant
        }

        public class ThreadStaticAttribute<T> : ThreadStaticAttribute { }
    }
}
