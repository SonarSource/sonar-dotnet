using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.Test.TestCases
{
    internal class UnnecessaryMathematicalComparison
    {
        public void IsPatterns()
        {
            _ = 33 is 55; // Compliant
            _ = 33 is < 55; // Compliant
            _ = 33 is <= 55; // Compliant
            _ = 33 is > 55; // Compliant
            _ = 33 is >= 55; // Compliant
            _ = 33 is not 55; // Compliant

            const double d = double.MaxValue;
            float x = 42;
            _ = x is d; // Error [CS0266] - cannot implicitly convert type 'double' to 'float'
        }
    }
}
