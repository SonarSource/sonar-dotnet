using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class LockOnThisOrType
    {
        public void MyLockingMethod()
        {
            lock ("""foo""") // Noncompliant
            {
            }
        }

        object lockObj = new object();
    }
}
