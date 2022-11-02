using System;
using System.Collections;

namespace Tests.Diagnostics
{
    public class SomeClass : ArrayList
    {
        public bool SomeMethod(SomeClass a)
        {
            return a is [SomeClass];
        }
    }
}
