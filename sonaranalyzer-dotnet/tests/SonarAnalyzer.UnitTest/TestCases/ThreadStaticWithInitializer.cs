using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class ThreadStaticWithInitializer
    {
        public class Foo
        {
            [ThreadStatic]
            public static object PerThreadObject = new object(); // Noncompliant {{Remove this initialization of 'PerThreadObject' or make it lazy.}}
//                                               ^^^^^^^^^^^^^^

            [ThreadStatic]
            public static object _perThreadObject;

            public static object StaticObject = new object();
        }
    }
}
