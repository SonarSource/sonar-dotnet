using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using NSonarQubeAnalyzer;
using System.Collections.Immutable;
using FluentAssertions;

namespace Tests.Diagnostics
{
    public class Verifier
    {
        public static void Verify(string path, IDiagnosticAnalyzer diagnosticAnalyzer)
        {
            var runner = new DiagnosticsRunner(ImmutableArray.Create(diagnosticAnalyzer));

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(path, Encoding.UTF8));

            List<int> expected = new List<int>(ExpectedIssues(syntaxTree));
            foreach (var diagnostic in runner.GetDiagnostics(syntaxTree))
            {
                if (diagnostic.Id == diagnosticAnalyzer.SupportedDiagnostics.Single().Id)
                {
                    int line = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1;
                    expected.Should().Contain(line);
                    expected.Remove(line);
                }
            }

            expected.Should().BeEmpty();
        }

        private static IEnumerable<int> ExpectedIssues(SyntaxTree syntaxTree)
        {
            var builder = ImmutableHashSet.CreateBuilder<int>();

            return from trivia in syntaxTree.GetRoot().DescendantTrivia()
                   where trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) &&
                   trivia.ToFullString().Contains("Noncompliant")
                   select trivia.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        }
    }
}
