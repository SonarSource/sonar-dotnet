using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class MlirGenerator : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1234";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                new DiagnosticDescriptor("S1234", string.Empty, string.Empty, "Sonar CBDE", DiagnosticSeverity.Hidden, true)
            );

        protected override void Initialize(SonarAnalysisContext context)
        {

        }
    }
}
