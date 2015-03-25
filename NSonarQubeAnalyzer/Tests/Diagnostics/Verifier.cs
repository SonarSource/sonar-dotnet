using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    public class Verifier
    {
        public static void Verify(string path, DiagnosticAnalyzer diagnosticAnalyzer)
        {
            var runner = new DiagnosticsRunner(ImmutableArray.Create(diagnosticAnalyzer));

            var solution =
                new AdhocWorkspace().CurrentSolution.AddProject("foo", "foo.dll", LanguageNames.CSharp)
                    .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof (object).Assembly))
                    .AddDocument("foo.cs", File.ReadAllText(path, Encoding.UTF8))
                    .Project
                    .Solution;
            
            var compilation = solution.Projects.First().GetCompilationAsync().Result;
            var syntaxTree = compilation.SyntaxTrees.First();
            
            //var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(path, Encoding.UTF8));

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

        private static IEnumerable<int> ExpectedIssues(SyntaxTree syntaxTree)
        {
            var builder = ImmutableHashSet.CreateBuilder<int>();

            return from l in syntaxTree.GetText().Lines
                   where l.ToString().Contains("Noncompliant")
                   select l.LineNumber + 1;
        }
    }
}
