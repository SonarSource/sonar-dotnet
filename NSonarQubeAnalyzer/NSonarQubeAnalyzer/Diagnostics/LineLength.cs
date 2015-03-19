using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NSonarQubeAnalyzer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LineLength : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "LineLength";
        internal const string Description = "Lines should not be too long";
        internal const string MessageFormat = "Split this {1} characters long line (which is greater than {0} authorized).";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public int Maximum;

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(
                c =>
                {
                    foreach (var line in c.Tree.GetText().Lines)
                    {
                        if (line.Span.Length > Maximum)
                        {
                            c.ReportDiagnostic(Diagnostic.Create(Rule, c.Tree.GetLocation(line.Span), Maximum, line.Span.Length));
                        }
                    }
                });
        }
    }
}
