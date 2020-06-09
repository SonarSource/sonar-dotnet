using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Helpers
{
    internal static class MethodDeclarationExtensions
    {
        /// <summary>
        /// Returns true if the method throws exceptions or returns null.
        /// </summary>
        internal static bool ThrowsOrReturnsNull(this MethodDeclarationSyntax syntaxNode) =>
            syntaxNode == null ||
            syntaxNode.DescendantNodes().OfType<ThrowStatementSyntax>().Any() ||
            syntaxNode.DescendantNodes().OfType<ExpressionSyntax>().Any(expression => expression.IsKind(SyntaxKindEx.ThrowExpression)) ||
            syntaxNode.DescendantNodes().OfType<ReturnStatementSyntax>().Any(returnStatement => returnStatement.Expression.IsKind(SyntaxKind.NullLiteralExpression)) ||
            // For simplicity this returns true for any method witch contains a NullLiteralExpression but this could be a source of FNs/FPs
            syntaxNode.DescendantNodes().OfType<ExpressionSyntax>().Any(expression => expression.IsKind(SyntaxKind.NullLiteralExpression));
    }
}
