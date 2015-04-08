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
    [SqaleSubCharacteristic(SqaleSubCharacteristic.LogicReliability)]
    [SqaleConstantRemediation("5min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class EmptyNestedBlock : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S108";
        internal const string Description = "Nested blocks of code should not be left empty";
        internal const string MessageFormat = "Either remove or fill this block of code.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category,
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: "http://nemo.sonarqube.org/coding_rules#rule_key=csharpsquid%3AS108");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (IsEmpty(c.Node))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation()));
                    }
                },
                SyntaxKind.Block,
                SyntaxKind.SwitchStatement);
        }

        private static bool IsEmpty(SyntaxNode node)
        {
            var switchNode = node as SwitchStatementSyntax;
            var blockNode = node as BlockSyntax;

            return (switchNode != null && IsEmpty(switchNode)) ||
                (blockNode != null && IsNestedAndEmpty(blockNode));
        }

        private static bool IsEmpty(SwitchStatementSyntax node)
        {
            return !node.Sections.Any();
        }

        private static bool IsNestedAndEmpty(BlockSyntax node)
        {
            return IsNested(node) && IsEmpty(node);
        }

        private static bool IsNested(BlockSyntax node)
        {
            return !AllowedContainerKinds.Contains(node.Parent.Kind());
        }

        private static IEnumerable<SyntaxKind> AllowedContainerKinds
        {
            get
            {
                return new[]
                {
                    SyntaxKind.ConstructorDeclaration,
                    SyntaxKind.DestructorDeclaration, 
                    SyntaxKind.MethodDeclaration,
                    SyntaxKind.SimpleLambdaExpression,
                    SyntaxKind.ParenthesizedLambdaExpression,
                    SyntaxKind.AnonymousMethodExpression
                };
            }
        }

        private static bool IsEmpty(BlockSyntax node)
        {
            return !node.Statements.Any() && !ContainsComment(node);
        }

        private static bool ContainsComment(BlockSyntax node)
        {
            return ContainsComment(node.OpenBraceToken.TrailingTrivia) || ContainsComment(node.CloseBraceToken.LeadingTrivia);
        }

        private static bool ContainsComment(SyntaxTriviaList trivias)
        {
            return trivias.Any(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));
        }
    }
}
