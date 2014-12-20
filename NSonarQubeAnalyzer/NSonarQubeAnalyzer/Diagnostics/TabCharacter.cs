namespace NSonarQubeAnalyzer.Diagnostics
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using System.Collections.Immutable;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TabCharacter : DiagnosticsRule
    {
        internal const string DiagnosticId = "TabCharacter";
        internal const string Description = "Tabulation character should not be used";
        internal const string MessageFormat = "Replace all tab characters in this file by sequences of white-spaces.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        /// <summary>
        /// Rule ID
        /// </summary>
        public override string RuleId
        {
            get
            {
                return "TabCharacter";
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            if (!Status)
            {
                return;
            }

            context.RegisterSyntaxTreeAction(
                c =>
                {
                    int offset = c.Tree.GetText().ToString().IndexOf('\t');
                    if (offset >= 0)
                    {
                        var location = c.Tree.GetLocation(TextSpan.FromBounds(offset, offset));
                        c.ReportDiagnostic(Diagnostic.Create(Rule, location));
                    }
                });
        }
    }
}