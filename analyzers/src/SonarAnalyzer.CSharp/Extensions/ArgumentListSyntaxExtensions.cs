/*
 * This is invalid licence header
 */

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Extensions
{
    public static class ArgumentListSyntaxExtensions
    {
        public static ExpressionSyntax Get(this ArgumentListSyntax argumentList, int index) =>
            argumentList != null && argumentList.Arguments.Count > index
                ? argumentList.Arguments[index].Expression.RemoveParentheses()
                : null;
    }
}
