using System.Collections.Immutable;
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
    [SqaleSubCharacteristic(SqaleSubCharacteristic.DataReliability)]
    [SqaleConstantRemediation("3min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [Tags("bug", "cert")]
    public class SelfAssignedVariables : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1656";
        internal const string Description = @"Variables should not be self-assigned";
        internal const string MessageFormat = @"Remove or correct this useless self-assignment";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category,
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: "http://nemo.sonarqube.org/coding_rules#rule_key=csharpsquid%3AS1656");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var expression = (AssignmentExpressionSyntax) c.Node;

                    if (expression.Parent is InitializerExpressionSyntax)
                    {
                        return;
                    }

                    using (var eqChecker = new EquivalenceChecker(c.SemanticModel))
                    {
                        if (eqChecker.AreEquivalent(expression.Left, expression.Right))
                        {
                            c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation()));
                        }
                    }
                },
                SyntaxKind.SimpleAssignmentExpression
                );
        }
    }
}