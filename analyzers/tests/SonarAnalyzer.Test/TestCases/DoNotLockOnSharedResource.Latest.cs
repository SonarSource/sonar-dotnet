using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class LockOnThisOrType(string myStringField)
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

        public void MyLockingMethod()
        {
            lock (myStringField) // Noncompliant
            {
            }
        }

        public void EscapeChar()
        {
            lock ("\e") // Noncompliant
            { }
        }
    }
}
