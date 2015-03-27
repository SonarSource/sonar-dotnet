using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace NSonarQubeAnalyzer.Diagnostics.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NoSqaleRemediation]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Readability)]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class LineLength : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "LineLength";
        internal const string Description = "Lines should not be too long";
        internal const string MessageFormat = "Split this {1} characters long line (which is greater than {0} authorized).";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Minor; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        [RuleParameter("maximumLineLength", PropertyType.Integer, "The maximum authorized line length", "200")]
        public int Maximum { get; set; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(
                c =>
                {
                    foreach (var line in c.Tree
                        .GetText()
                        .Lines
                        .Where(line => line.Span.Length > Maximum))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, c.Tree.GetLocation(line.Span), Maximum, line.Span.Length));
                    }
                });
        }
    }
}
