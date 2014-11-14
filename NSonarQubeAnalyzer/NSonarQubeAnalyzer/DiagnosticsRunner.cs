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
        private readonly ImmutableArray<IDiagnosticAnalyzer> DiagnosticAnalyzers;

        public DiagnosticsRunner(ImmutableArray<IDiagnosticAnalyzer> diagnosticAnalyzers)
        {
            DiagnosticAnalyzers = diagnosticAnalyzers;
        }

        public IEnumerable<Diagnostic> GetDiagnostics(SyntaxTree syntaxTree)
        {
            var cancellationToken = new CancellationTokenSource().Token;
            // FIXME Cannot dispose() because of violation of invariant in
            // http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis/DiagnosticAnalyzer/AsyncQueue.cs,187
            var driver = new AnalyzerDriver<SyntaxKind>(DiagnosticAnalyzers, n => n.CSharpKind(), null, cancellationToken, null);

            Compilation compilation = CSharpCompilation.Create(null, ImmutableArray.Create(syntaxTree));
            compilation = compilation.WithEventQueue(driver.CompilationEventQueue);
            compilation.GetDiagnostics(cancellationToken);

            return driver.GetDiagnosticsAsync().Result;
        }
    }
}
