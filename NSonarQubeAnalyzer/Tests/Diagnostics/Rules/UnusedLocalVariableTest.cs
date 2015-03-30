using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class UnusedLocalVariableTest
    {
        [TestMethod]
        public void UnusedLocalVariable()
        {
            var solution = CompilationHelper.GetSolutionFromFiles(@"TestCases\UnusedLocalVariable.cs");

            Verifier.Verify(solution, new UnusedLocalVariable() { CurrentSolution = solution });
        }

        
    }
}
