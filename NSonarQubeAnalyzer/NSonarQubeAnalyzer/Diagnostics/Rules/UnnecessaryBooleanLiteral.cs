using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace NSonarQubeAnalyzer.Diagnostics.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Readability)]
    [SqaleConstantRemediation("2min")]
    [Tags("clumsy")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class UnnecessaryBooleanLiteral : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1125";
        internal const string Description = "Literal boolean values should not be used in condition expressions";
        internal const string MessageFormat = "Remove the literal \"{0}\" boolean value.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Minor; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var literalNode = (LiteralExpressionSyntax)c.Node;
                    if (IsUnnecessary(literalNode))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, literalNode.GetLocation(), literalNode.Token.ToString()));
                    }
                },
                SyntaxKind.TrueLiteralExpression,
                SyntaxKind.FalseLiteralExpression);
        }

        private static bool IsUnnecessary(LiteralExpressionSyntax node)
        {
            return node.Parent.IsKind(SyntaxKind.EqualsExpression) ||
                node.Parent.IsKind(SyntaxKind.NotEqualsExpression) ||
                node.Parent.IsKind(SyntaxKind.LogicalAndExpression) ||
                node.Parent.IsKind(SyntaxKind.LogicalOrExpression) ||
                node.Parent.IsKind(SyntaxKind.LogicalNotExpression);
        }
    }
}
