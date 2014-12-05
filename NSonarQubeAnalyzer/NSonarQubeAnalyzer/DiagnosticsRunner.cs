using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NSonarQubeAnalyzer
{
    public class DiagnosticsRunner
    {
        private readonly ImmutableArray<DiagnosticAnalyzer> DiagnosticAnalyzers;

        public DiagnosticsRunner(ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers)
        {
            DiagnosticAnalyzers = diagnosticAnalyzers;
        }

        public IEnumerable<Diagnostic> GetDiagnostics(SyntaxTree syntaxTree)
        {
            var cancellationToken = new CancellationTokenSource().Token;

            Compilation compilation = CSharpCompilation.Create(null, ImmutableArray.Create(syntaxTree));

            var driver = AnalyzerDriver.Create(compilation, DiagnosticAnalyzers, null, out compilation, cancellationToken);
            compilation.GetDiagnostics(cancellationToken);
            return driver.GetDiagnosticsAsync().Result;
        }
    }
}
