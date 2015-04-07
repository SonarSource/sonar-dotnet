using System.Collections.Generic;
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

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault);

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
            return AllowedContainerKinds.Contains(node.Parent.Kind());
        }

        private static IEnumerable<SyntaxKind> AllowedContainerKinds
        {
            get
            {
                return new[]
                {
                    SyntaxKind.EqualsExpression,
                    SyntaxKind.NotEqualsExpression, 
                    SyntaxKind.LogicalAndExpression,
                    SyntaxKind.LogicalOrExpression,
                    SyntaxKind.LogicalNotExpression
                };
            }
        }
    }
}
