namespace NSonarQubeAnalyzer.Diagnostics
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SwitchWithoutDefault : DiagnosticsRule
    {
        internal const string DiagnosticId = "SwitchWithoutDefault";
        internal const string Description = "'switch' statement should have a 'default:' case";
        internal const string MessageFormat = "Add a default: case to this switch.";
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
                return "SwitchWithoutDefault";
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    SwitchStatementSyntax switchNode = (SwitchStatementSyntax)c.Node;
                    if (!HasDefaultLabel(switchNode))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, switchNode.GetLocation()));
                    }
                },
                SyntaxKind.SwitchStatement);
        }

        private static bool HasDefaultLabel(SwitchStatementSyntax node)
        {
            return node.Sections.Any(section => section.Labels.Any(labels => labels.IsKind(SyntaxKind.DefaultSwitchLabel)));
        }
    }
}