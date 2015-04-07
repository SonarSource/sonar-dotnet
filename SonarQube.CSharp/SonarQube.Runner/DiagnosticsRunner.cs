using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarQube.Runner
{
    public class DiagnosticsRunner
    {
        private readonly ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers;
        
        public DiagnosticsRunner(ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers)
        {
			foreach (var analyzer in diagnosticAnalyzers)
			{
				foreach (var diagnostic in analyzer.SupportedDiagnostics)
				{
					diagnostic.GetType().GetProperty("IsEnabledByDefault").SetValue(diagnostic, true);
				}	
			}

			this.diagnosticAnalyzers = diagnosticAnalyzers;
		}

        public IEnumerable<Diagnostic> GetDiagnostics(Compilation compilation)
        {
            if (diagnosticAnalyzers.IsDefaultOrEmpty)
            {
                return new Diagnostic[0];
            }

            using (var tokenSource = new CancellationTokenSource())
            {
                var compilationWithAnalyzer = new CompilationWithAnalyzers(compilation, diagnosticAnalyzers, null,
                    tokenSource.Token);

                return compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync().Result;
            }
        }
    }
}
