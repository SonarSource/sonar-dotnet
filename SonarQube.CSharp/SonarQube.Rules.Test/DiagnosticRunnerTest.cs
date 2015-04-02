using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Runner;

namespace SonarQube.Rules.Test
{
    [TestClass]
    public class DiagnosticRunnerTest
    {
        [TestMethod]
        public void DiagnosticRunnerTest_NoAnalyzer()
        {
            var runner = new DiagnosticsRunner(ImmutableArray.Create<DiagnosticAnalyzer>());

            var solution = CompilationHelper.GetSolutionFromText("");

            var compilation = solution.Projects.First().GetCompilationAsync().Result;
            var syntaxTree = compilation.SyntaxTrees.First();


            var diagnosticsResult = runner.GetDiagnostics(compilation);

            diagnosticsResult.Should().HaveCount(0);
        }
    }
}
