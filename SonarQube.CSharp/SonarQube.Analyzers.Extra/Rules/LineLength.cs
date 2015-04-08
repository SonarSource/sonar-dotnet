using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarQube.Analyzers.Helpers;
using SonarQube.Analyzers.SonarQube.Settings;
using SonarQube.Analyzers.SonarQube.Settings.Sqale;

namespace SonarQube.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleConstantRemediation("1min")]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Readability)]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [LegacyKey("LineLength")]
    [Tags("convention")]
    public class LineLength : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S103";
        internal const string Description = "Lines should not be too long";
        internal const string MessageFormat = "Split this {1} characters long line (which is greater than {0} authorized).";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Minor; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category,
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: "http://nemo.sonarqube.org/coding_rules#rule_key=csharpsquid%3ALineLength");

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
