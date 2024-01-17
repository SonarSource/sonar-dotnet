using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.Test.TestCases.SymbolicExecution.Roslyn
{
    public class NullPointerDereference
    {
        [Obsolete(nameof(argument.Trim))] // Compliant
        public void ExtendedScopeNameOfInAttribute(string argument)
        {
            string nullArgument = null;
            nullArgument.Trim(); // Noncompliant
        }

        public void ListPattern(object[] objects)
        {
            if (objects is [null, null, null])
            {
                objects[1].GetHashCode(); // FN
            }
        }
    }
}
