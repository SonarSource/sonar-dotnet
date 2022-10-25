using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class LockOnThisOrType
    {
        public void RawStringLiterals()
        {
            lock ("""foo""") // Noncompliant
            {
            }
        }
    }
}
