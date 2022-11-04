using System;

namespace Tests.Diagnostics
{
    public class LockOnThisOrType
    {
        public void RawStringLiterals()
        {
            lock ("""foo""") { }// Noncompliant
        }

        void NewlinesInStringInterpolation()
        {
            string s = "test";
            lock ($"{s
                .ToUpper()
                }")
            { }
            // Noncompliant@-4
        }
    }
}
