using System;

namespace SonarAnalyzer.Test.TestCases.SymbolicExecution.Sonar
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

    // https://github.com/SonarSource/sonar-dotnet/issues/6766
    public class Repo_6766
    {
        public void SwitchCasePattern(object o, object[] array)
        {
            switch (o)
            {
                case (1 or 2):                        // Parenthesized
                case 3 or 4:                          // Binary
                case > 5:                             // Relational
                case char:                            // Type
                case Exception { Message.Length: 1 }: // Recursive
                case (int _, int _):                  // Recursive
                case not "":                          // Unary
                    break;
            }
            switch (array)
            {
                case []:         // list pattern
                case [1, .., 2]: // list pattern with slice
                    break;
            }
        }
    }
}
