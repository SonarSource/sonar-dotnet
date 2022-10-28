using System;

namespace Tests.Diagnostics
{
    public class LockOnThisOrType
    {
        public void RawStringLiterals()
        {
            lock ("""foo""") { }// Noncompliant
        }
    }
}
