using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class LockOnThisOrType
    {
        public void MyLockingMethod()
        {
            lock (this) // Noncompliant
//                ^^^^
            {
                // ...
            }

            lock (lockObj)
            {
                // ...
            }

            lock (typeof(LockOnThisOrType)) // Noncompliant
//                ^^^^^^^^^^^^^^^^^^^^^^^^
            {
                // ...
            }
            lock ((new LockOnThisOrType()).GetType()) // Noncompliant {{Lock on a dedicated object instance instead.}}
            {
                // ...
            }
        }

        object lockObj = new object();
    }
}
