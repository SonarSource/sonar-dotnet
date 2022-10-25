using System;

namespace Tests.Diagnostics
{
    public class ThreadStaticWithInitializer
    {
        public class Foo
        {
            [ThreadStaticAttribute<int>]
            public static object PerThreadObject = new object(); // FN
        }

        public class ThreadStaticAttribute<T> : ThreadStaticAttribute { }
    }
}
