using System;

namespace Tests.Diagnostics
{
    public class ThreadStaticWithInitializer
    {
        public class Foo
        {
            [ThreadStaticAttribute<int>]
            public static object PerThreadObject = new object(); // FN for performance reasons we decided not to handle derived classes
        }

        public class ThreadStaticAttribute<T> : ThreadStaticAttribute { }
    }
}
