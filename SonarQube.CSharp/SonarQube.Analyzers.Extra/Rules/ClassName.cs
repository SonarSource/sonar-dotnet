using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarQube.Analyzers.Helpers;
using SonarQube.Analyzers.SonarQube.Settings;
using SonarQube.Analyzers.SonarQube.Settings.Sqale;

namespace SonarQube.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Readability)]
    [SqaleConstantRemediation("5min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [LegacyKey("ClassName")]
    public class ClassName : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S101";
        internal const string Description = "Class name should comply with a naming convention";
        internal const string MessageFormat = "Rename this class to match the regular expression: {0}";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Minor;
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category,
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: "http://nemo.sonarqube.org/coding_rules#rule_key=csharpsquid%3AClassName");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        [RuleParameter("format", PropertyType.String, "Regular expression used to check the class names against.", "^(?:[A-HJ-Z][a-zA-Z0-9]+|I[a-z0-9][a-zA-Z0-9]*)$")]
        public string Convention { get; set; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var identifier = ((ClassDeclarationSyntax)c.Node).Identifier;

                    if (!Regex.IsMatch(identifier.Text, Convention))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), Convention));
                    }
                },
                SyntaxKind.ClassDeclaration);
        }
    }
}
