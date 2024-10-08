using System;
using System.Threading;

namespace Tests.Diagnostics
{
    class Program
    {
        private string rawStringLiterals = """some value""";

        private void Test()
        {
            lock (rawStringLiterals) { } // Noncompliant
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
