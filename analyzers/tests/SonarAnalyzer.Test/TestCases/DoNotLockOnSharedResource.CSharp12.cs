using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class LockOnThisOrType(string myStringField)
    {
        public void MyLockingMethod()
        {
            lock (myStringField) // Noncompliant
            {
            }
        }
    }
}
