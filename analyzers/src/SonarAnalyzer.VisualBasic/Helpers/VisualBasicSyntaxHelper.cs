/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Helpers;

internal static class VisualBasicSyntaxHelper
{
    private static readonly SyntaxKind[] LiteralSyntaxKinds =
        [
            SyntaxKind.CharacterLiteralExpression,
            SyntaxKind.FalseLiteralExpression,
            SyntaxKind.NothingLiteralExpression,
            SyntaxKind.NumericLiteralExpression,
            SyntaxKind.StringLiteralExpression,
            SyntaxKind.TrueLiteralExpression,
        ];

    public static SyntaxNode GetTopMostContainingMethod(this SyntaxNode node) =>
        node.AncestorsAndSelf().LastOrDefault(x => x is MethodBaseSyntax || x is PropertyBlockSyntax);

    public static SyntaxNode RemoveParentheses(this SyntaxNode expression)
    {
        var current = expression;
        while (current is ParenthesizedExpressionSyntax parenthesized)
        {
            current = parenthesized.Expression;
        }
        return current;
    }

    public static ExpressionSyntax RemoveParentheses(this ExpressionSyntax expression) =>
        (ExpressionSyntax)RemoveParentheses((SyntaxNode)expression);

    public static SyntaxNode GetSelfOrTopParenthesizedExpression(this SyntaxNode node)
    {
        var current = node;
        while (current?.Parent?.IsKind(SyntaxKind.ParenthesizedExpression) ?? false)
        {
            current = current.Parent;
        }
        return current;
    }

    public static ExpressionSyntax GetSelfOrTopParenthesizedExpression(this ExpressionSyntax expression) =>
        (ExpressionSyntax)GetSelfOrTopParenthesizedExpression((SyntaxNode)expression);

    public static SyntaxNode GetFirstNonParenthesizedParent(this SyntaxNode node) =>
        node.GetSelfOrTopParenthesizedExpression().Parent;

    #region Statement

    public static StatementSyntax GetPrecedingStatement(this StatementSyntax currentStatement)
    {
        var children = currentStatement.Parent.ChildNodes().ToList();
        var index = children.IndexOf(currentStatement);
        return index == 0 ? null : children[index - 1] as StatementSyntax;
    }

    public static StatementSyntax GetSucceedingStatement(this StatementSyntax currentStatement)
    {
        var children = currentStatement.Parent.ChildNodes().ToList();
        var index = children.IndexOf(currentStatement);
        return index == children.Count - 1 ? null : children[index + 1] as StatementSyntax;
    }

    #endregion Statement

    public static bool HasAncestor(this SyntaxNode syntaxNode, params SyntaxKind[] syntaxKinds) =>
        syntaxNode.Ancestors().Any(x => x.IsAnyKind(syntaxKinds));

    public static bool IsNothingLiteral(this SyntaxNode syntaxNode) =>
        syntaxNode is not null && syntaxNode.IsKind(SyntaxKind.NothingLiteralExpression);

    public static bool IsAnyKind(this SyntaxNode syntaxNode, params SyntaxKind[] syntaxKinds) =>
       syntaxNode is not null && syntaxKinds.Contains((SyntaxKind)syntaxNode.RawKind);

    public static bool IsAnyKind(this SyntaxToken syntaxToken, ISet<SyntaxKind> collection) =>
        collection.Contains((SyntaxKind)syntaxToken.RawKind);

    public static bool IsAnyKind(this SyntaxNode syntaxNode, ISet<SyntaxKind> collection) =>
        syntaxNode is not null && collection.Contains((SyntaxKind)syntaxNode.RawKind);

    public static bool IsAnyKind(this SyntaxToken syntaxToken, params SyntaxKind[] syntaxKinds) =>
        syntaxKinds.Contains((SyntaxKind)syntaxToken.RawKind);

    public static bool IsAnyKind(this SyntaxTrivia syntaxTrivia, params SyntaxKind[] syntaxKinds) =>
        syntaxKinds.Contains((SyntaxKind)syntaxTrivia.RawKind);

    public static bool AnyOfKind(this IEnumerable<SyntaxNode> nodes, SyntaxKind kind) =>
        nodes.Any(x => x.RawKind == (int)kind);

    public static bool IsMethodInvocation(this InvocationExpressionSyntax expression, KnownType type, string methodName, SemanticModel semanticModel) =>
        semanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol methodSymbol &&
        methodSymbol.IsInType(type) &&
        // vbnet is case insensitive
        methodName.Equals(methodSymbol.Name, StringComparison.InvariantCultureIgnoreCase);

    public static bool IsOnBase(this ExpressionSyntax expression) =>
        IsOn(expression, SyntaxKind.MyBaseExpression);

    private static bool IsOn(this ExpressionSyntax expression, SyntaxKind onKind) =>
        expression switch
        {
            InvocationExpressionSyntax invocation => IsOn(invocation.Expression, onKind),
            // This is a simplification as we don't check where the method is defined (so this could be this or base)
            GlobalNameSyntax or GenericNameSyntax or IdentifierNameSyntax or QualifiedNameSyntax => true,
            MemberAccessExpressionSyntax memberAccessExpression when memberAccessExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression) =>
                memberAccessExpression.Expression.RemoveParentheses().IsKind(onKind),
            ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.Expression.RemoveParentheses().IsKind(onKind),
            _ => false,
        };

    public static string GetName(this SyntaxNode expression) =>
        expression.GetIdentifier()?.ValueText ?? string.Empty;

    public static bool NameIs(this ExpressionSyntax expression, string name) =>
        expression.GetName().Equals(name, StringComparison.InvariantCultureIgnoreCase);

    public static bool HasConstantValue(this ExpressionSyntax expression, SemanticModel semanticModel) =>
        expression.RemoveParentheses().IsAnyKind(LiteralSyntaxKinds) || expression.FindConstantValue(semanticModel) is not null;

    public static string StringValue(this SyntaxNode node, SemanticModel semanticModel) =>
        node switch
        {
            LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) => literal.Token.ValueText,
            InterpolatedStringExpressionSyntax expression => expression.TryGetInterpolatedTextValue(semanticModel, out var interpolatedValue) ? interpolatedValue : expression.GetContentsText(),
            _ => null
        };

    public static bool IsLeftSideOfAssignment(this ExpressionSyntax expression)
    {
        var topParenthesizedExpression = expression.GetSelfOrTopParenthesizedExpression();
        return topParenthesizedExpression.Parent.IsKind(SyntaxKind.SimpleAssignmentStatement) &&
            topParenthesizedExpression.Parent is AssignmentStatementSyntax assignment &&
            assignment.Left == topParenthesizedExpression;
    }

    public static bool IsComment(this SyntaxTrivia trivia) =>
        trivia.IsAnyKind(
            SyntaxKind.CommentTrivia,
            SyntaxKind.DocumentationCommentExteriorTrivia,
            SyntaxKind.DocumentationCommentTrivia);

    public static Location FindIdentifierLocation(this MethodBlockBaseSyntax methodBlockBase) =>
        GetIdentifierOrDefault(methodBlockBase)?.GetLocation();

    public static SyntaxToken? GetIdentifierOrDefault(this MethodBlockBaseSyntax methodBlockBase) =>
        methodBlockBase?.BlockStatement switch
        {
            SubNewStatementSyntax subNewStatement => subNewStatement.NewKeyword,
            MethodStatementSyntax methodStatement => methodStatement.Identifier,
            _ => null,
        };

    public static string GetIdentifierText(this MethodBlockSyntax method) =>
        method.SubOrFunctionStatement.Identifier.ValueText;

    public static SeparatedSyntaxList<ParameterSyntax>? GetParameters(this MethodBlockSyntax method) =>
        method.BlockStatement?.ParameterList?.Parameters;

    public static ExpressionSyntax Get(this ArgumentListSyntax argumentList, int index) =>
        argumentList is not null && argumentList.Arguments.Count > index
            ? argumentList.Arguments[index].GetExpression().RemoveParentheses()
            : null;

    /// <summary>
    /// Returns argument expressions for given parameter.
    ///
    /// There can be zero, one or more results based on parameter type (Optional or ParamArray/params).
    /// </summary>
    public static ImmutableArray<SyntaxNode> ArgumentValuesForParameter(SemanticModel semanticModel, ArgumentListSyntax argumentList, string parameterName) =>
        argumentList is not null
            && new VisualBasicMethodParameterLookup(argumentList, semanticModel).TryGetSyntax(parameterName, out var expressions)
                ? expressions
                : ImmutableArray<SyntaxNode>.Empty;
}
