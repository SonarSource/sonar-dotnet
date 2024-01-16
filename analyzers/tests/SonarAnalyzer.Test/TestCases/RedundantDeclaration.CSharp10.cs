using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    abstract class RedundantDeclaration
    {
        public void M()
        {
            Test(null, new BoolDelegate(() => true)); // Noncompliant
        }

        public abstract void Test(object o, Delegate f);
        public delegate bool BoolDelegate();
    }
}
