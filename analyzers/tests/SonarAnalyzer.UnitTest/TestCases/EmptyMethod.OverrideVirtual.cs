using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public class FooBase
    {
        public virtual void Method()
        {

        }
    }

    public class FooImpl: FooBase
    {
        public override void Method() // Noncompliant
        {

        }
    }
}
