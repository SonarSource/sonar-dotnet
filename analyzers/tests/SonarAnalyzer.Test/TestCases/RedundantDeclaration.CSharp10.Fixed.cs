using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    abstract class RedundantDeclaration
    {
        public void M()
        {
            Test(null, new BoolDelegate(() => true)); // Compliant (natural type is Func<bool> and not BoolDelegate)
            Test(null, () => true);   // Fixed
        }

        public abstract void Test(object o, Delegate f);
        public delegate bool BoolDelegate();
    }
}
