using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Extensions
{
    public static class ExpressionSyntaxExtensions
    {
        public static ExpressionSyntax RemoveParentheses(this ExpressionSyntax expression) =>
            (ExpressionSyntax)((SyntaxNode)expression).RemoveParentheses();
    }
}
