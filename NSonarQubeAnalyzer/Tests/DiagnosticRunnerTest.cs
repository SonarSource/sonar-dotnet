using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests
{
    [TestClass]
    public class DiagnosticRunnerTest
    {
        [TestMethod]
        public void DiagnosticRunnerTest_NoAnalyzer()
        {
            var runner = new DiagnosticsRunner(ImmutableArray.Create<DiagnosticAnalyzer>());

            var solution =
                new AdhocWorkspace().CurrentSolution.AddProject("foo", "foo.dll", LanguageNames.CSharp)
                    .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof (object).Assembly))
                    .AddDocument("foo.cs", "")
                    .Project
                    .Solution;

            var compilation = solution.Projects.First().GetCompilationAsync().Result;
            var syntaxTree = compilation.SyntaxTrees.First();


            var diagnosticsResult = runner.GetDiagnostics(compilation);

            diagnosticsResult.Count().ShouldBeEquivalentTo(0);
        }
    }
}
