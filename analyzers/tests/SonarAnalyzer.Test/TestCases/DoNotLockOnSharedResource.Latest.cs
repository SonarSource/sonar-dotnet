using System;
using System.Collections.Generic;
using System.Threading;

namespace Tests.Diagnostics
{
    public class LockOnThisOrType(string myStringField)
    {
        public void RawStringLiterals()
        {
            lock ("""foo""") // Noncompliant
            { }
        }

        void NewlinesInStringInterpolation()
        {
            string s = "test";
            lock ($"{s
                .ToUpper()}")
            { }
            // Noncompliant@-3
        }

        public void MyLockingMethod()
        {
            lock (myStringField) // Noncompliant
            { }
        }

        public void EscapeChar()
        {
            lock ("\e") // Noncompliant
            { }
        }
    }

    public class LockOnLockType
    {
        private Lock _lock;
        public void LockOnLock()
        {
            lock (_lock) // Compliant
            { }
        }
    }
}
