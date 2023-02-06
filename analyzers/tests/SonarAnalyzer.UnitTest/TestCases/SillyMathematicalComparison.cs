using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.UnitTest.TestCases
{
    internal class SillyMathematicalComparison
    {
        public void Floats()
        {
            float f = 42;

            const double veryBig = double.MaxValue;
            const double verySmall = double.MinValue;

            _ = f < veryBig; // Noncompliant
            _ = veryBig > f; // Noncompliant
            _ = f > verySmall; // Noncompliant
            _ = verySmall < f; // Noncompliant
        }
    }
}
