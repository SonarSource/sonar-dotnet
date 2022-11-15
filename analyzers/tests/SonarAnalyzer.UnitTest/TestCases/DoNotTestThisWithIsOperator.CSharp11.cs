using System;
using System.Collections;

namespace Tests.Diagnostics
{
    public class SomeClass : ArrayList
    {
        public void SomeMethod()
        {
            var a = this is [1, 2, 3]; // Noncompliant
        }
    }
}
