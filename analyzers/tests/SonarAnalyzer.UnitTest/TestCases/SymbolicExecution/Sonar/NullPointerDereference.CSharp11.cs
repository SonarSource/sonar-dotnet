using System;

namespace SonarAnalyzer.UnitTest.TestCases.SymbolicExecution.Sonar
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
