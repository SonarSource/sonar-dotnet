using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.UnitTest.TestCases
{
    internal class SillyMathematicalComparison
    {
        public void IsPatterns()
        {
            _ = 33 is 55; // Compliant
            _ = 33 is < 55; // Compliant
            _ = 33 is <= 55; // Compliant
            _ = 33 is > 55; // Compliant
            _ = 33 is >= 55; // Compliant
            _ = 33 is not 55; // Compliant
        }
    }
}
