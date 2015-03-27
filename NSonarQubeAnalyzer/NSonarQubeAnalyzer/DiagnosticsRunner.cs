using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NSonarQubeAnalyzer
{
    public class DiagnosticsRunner
    {
        private readonly ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers;
        
        public DiagnosticsRunner(ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers)
        {
            this.diagnosticAnalyzers = diagnosticAnalyzers;
        }

        public IEnumerable<Diagnostic> GetDiagnostics(Compilation compilation)
        {
            var cancellationToken = new CancellationTokenSource().Token;
            
            if (diagnosticAnalyzers.IsDefaultOrEmpty)
            {
                return new Diagnostic[0];
            }

            var compilationWithAnalyzer = new CompilationWithAnalyzers(compilation, diagnosticAnalyzers, null, cancellationToken);

            return compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync().Result;
        }
    }
}
