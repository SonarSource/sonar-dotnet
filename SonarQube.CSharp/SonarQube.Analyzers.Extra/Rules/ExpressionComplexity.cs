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
    [SqaleConstantRemediation("30min")]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.UnitTestability)]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class ExpressionComplexity : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1067";
        internal const string Description = "Expressions should not be too complex";
        internal const string MessageFormat = "Reduce the number of conditional operators ({1}) used in the expression (maximum allowed {0}).";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category,
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: "http://nemo.sonarqube.org/coding_rules#rule_key=csharpsquid%3AS1067");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        [RuleParameter("max", PropertyType.Integer, "Maximum number of allowed conditional operators in an expression", "3")]
        public int Maximum { get; set; }

        private readonly IImmutableSet<SyntaxKind> compoundExpressionKinds = ImmutableHashSet.Create(
            SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.ArrayInitializerExpression,
            SyntaxKind.AnonymousMethodExpression,
            SyntaxKind.ObjectInitializerExpression,
            SyntaxKind.InvocationExpression);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var rootExpressions =
                        c.Node
                        .DescendantNodes(e2 => !(e2 is ExpressionSyntax))
                        .Where(
                            e =>
                                e is ExpressionSyntax &&
                                !IsCompoundExpression(e));

                    var compoundExpressionsDescendants =
                        c.Node
                        .DescendantNodes()
                        .Where(IsCompoundExpression)
                        .SelectMany(
                            e =>
                                e
                                .DescendantNodes(
                                    e2 =>
                                        e == e2 ||
                                        !(e2 is ExpressionSyntax))
                                .Where(
                                    e2 =>
                                        e2 is ExpressionSyntax &&
                                        !IsCompoundExpression(e2)));

                    var expressionsToCheck = rootExpressions.Concat(compoundExpressionsDescendants);

                    var complexExpressions =
                        expressionsToCheck
                        .Select(
                            e =>
                            new
                            {
                                Expression = e,
                                Complexity =
                                    e
                                    .DescendantNodesAndSelf(e2 => !IsCompoundExpression(e2))
                                    .Count(
                                        e2 =>
                                            e2.IsKind(SyntaxKind.ConditionalExpression) ||
                                            e2.IsKind(SyntaxKind.LogicalAndExpression) ||
                                            e2.IsKind(SyntaxKind.LogicalOrExpression))
                            })
                        .Where(e => e.Complexity > Maximum);

                    foreach (var complexExpression in complexExpressions)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, complexExpression.Expression.GetLocation(), Maximum, complexExpression.Complexity));
                    }
                },
                SyntaxKind.CompilationUnit);
        }

        private bool IsCompoundExpression(SyntaxNode node)
        {
            return compoundExpressionKinds.Any(node.IsKind);
        }
    }
}
