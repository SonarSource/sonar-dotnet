using System;
using System.Globalization;
using System.Resources;

namespace Tests.Diagnostics
{
    class Program
    {
        void TestCases()
        {
            int result;
            int.TryParse("123", out result); // Noncompliant
        }
    }
}
