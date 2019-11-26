using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class MyTestCases
    {
        public void Method1()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
                throw new InvalidOperationException(); // Noncompliant
            }
        }
    }
}
