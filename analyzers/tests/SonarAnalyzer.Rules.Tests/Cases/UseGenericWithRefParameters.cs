using System;

namespace Tests.Diagnostics
{
    public class Foo
    {
        public void Bar(ref object o1, ref object o2)
//                  ^^^ Noncompliant {{Make this method generic and replace the 'object' parameter with a type parameter.}}
//                                 ^^ Secondary@-1
//                                                ^^ Secondary@-2
        {
        }

        public void Bar2<T>(ref T ref1, ref T ref2)
        {
        }

        public void Bar3(out object o1,
                         ref Foo o2,
                         ref object[] o3)
        {
            o1 = null;
        }
    }
}
