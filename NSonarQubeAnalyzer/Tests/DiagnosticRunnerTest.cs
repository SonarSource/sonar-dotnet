using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NSonarQubeAnalyzer;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Tests
{
    [TestClass]
    public class DiagnosticRunnerTest
    {
        [TestMethod]
        public void DiagnosticRunnerTest_NoAnalyzer()
        {
            var runner = new DiagnosticsRunner(ImmutableArray.Create<DiagnosticAnalyzer>());
            var syntaxTree = CSharpSyntaxTree.ParseText("");

            var diagnosticsResult = runner.GetDiagnostics(syntaxTree);

            diagnosticsResult.Count().ShouldBeEquivalentTo(0);
        }
    }
}
