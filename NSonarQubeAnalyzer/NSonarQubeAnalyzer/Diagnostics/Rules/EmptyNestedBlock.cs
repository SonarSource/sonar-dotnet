using System.Collections.Immutable;
using System.Linq;
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

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

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
            return (node is SwitchStatementSyntax && IsEmpty((SwitchStatementSyntax)node)) ||
                (node is BlockSyntax && IsNestedAndEmpty((BlockSyntax)node));
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
            var parent = node.Parent;
            return !parent.IsKind(SyntaxKind.ConstructorDeclaration) &&
                !parent.IsKind(SyntaxKind.DestructorDeclaration) &&
                !parent.IsKind(SyntaxKind.MethodDeclaration) &&
                !parent.IsKind(SyntaxKind.SimpleLambdaExpression) &&
                !parent.IsKind(SyntaxKind.ParenthesizedLambdaExpression) &&
                !parent.IsKind(SyntaxKind.AnonymousMethodExpression);
        }

        private static bool IsEmpty(BlockSyntax node)
        {
            return !node.Statements.Any() && !ContainsComment(node);
        }

        public static bool ContainsComment(BlockSyntax node)
        {
            return ContainsComment(node.OpenBraceToken.TrailingTrivia) || ContainsComment(node.CloseBraceToken.LeadingTrivia);
        }

        private static bool ContainsComment(SyntaxTriviaList trivias)
        {
            return trivias.Any(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));
        }
    }
}
