using System.Collections.Immutable;
using System.Linq;
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
    [SqaleConstantRemediation("5min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.LogicReliability)]
    [Tags("cert", "cwe", "misra")]
    [LegacyKey("SwitchWithoutDefault")]
    public class SwitchWithoutDefault : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S131";
        internal const string Description = "'switch' statement should have a 'default:' case";
        internal const string MessageFormat = "Add a default: case to this switch.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category,
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: "http://nemo.sonarqube.org/coding_rules#rule_key=csharpsquid%3ASwitchWithoutDefault");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var switchNode = (SwitchStatementSyntax)c.Node;
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
