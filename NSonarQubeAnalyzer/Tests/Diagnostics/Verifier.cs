using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    public class Verifier
    {
        public static void Verify(Solution solution, DiagnosticAnalyzer diagnosticAnalyzer)
        {
            var runner = new DiagnosticsRunner(ImmutableArray.Create(diagnosticAnalyzer));
            
            var compilation = solution.Projects.First().GetCompilationAsync().Result;
            var syntaxTree = compilation.SyntaxTrees.First();
            
            var expected = new List<int>(ExpectedIssues(syntaxTree));
            foreach (var diagnostic in runner.GetDiagnostics(compilation))
            {
                if (diagnostic.Id != diagnosticAnalyzer.SupportedDiagnostics.Single().Id)
                {
                    continue;
                }

                var line = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1;
                expected.Should().Contain(line);
                expected.Remove(line);
            }

            expected.Should().BeEquivalentTo(Enumerable.Empty<int>());
        }

        public static void Verify(string path, DiagnosticAnalyzer diagnosticAnalyzer)
        {
            var solution = CompilationHelper.GetSolutionFromFiles(path);
            Verify(solution, diagnosticAnalyzer);
        }

        private static IEnumerable<int> ExpectedIssues(SyntaxTree syntaxTree)
        {
            return from l in syntaxTree.GetText().Lines
                   where l.ToString().Contains("Noncompliant")
                   select l.LineNumber + 1;
        }
    }
}
