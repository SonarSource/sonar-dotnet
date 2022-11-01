using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.UnitTest.TestCases.SymbolicExecution.Roslyn
{
    public class NullPointerDereference
    {
        [Obsolete(nameof(argument.Trim))] // Compliant
        public void ExtendedScopeNameOfInAttribute(string argument)
        {
            string nullArgument = null;
            nullArgument.Trim(); // Noncompliant
        }
    }
}
