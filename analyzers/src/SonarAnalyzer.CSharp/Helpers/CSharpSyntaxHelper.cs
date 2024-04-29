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

public static class CSharpSyntaxHelper
{
    public static readonly ExpressionSyntax NullLiteralExpression =
        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

    public static readonly ExpressionSyntax FalseLiteralExpression =
        SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);

    public static readonly ExpressionSyntax TrueLiteralExpression =
        SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);

    public static readonly string NameOfKeywordText =
        SyntaxFacts.GetText(SyntaxKind.NameOfKeyword);

    private static readonly SyntaxKind[] LiteralSyntaxKinds =
        [
            SyntaxKind.CharacterLiteralExpression,
            SyntaxKind.FalseLiteralExpression,
            SyntaxKind.NullLiteralExpression,
            SyntaxKind.NumericLiteralExpression,
            SyntaxKind.StringLiteralExpression,
            SyntaxKind.TrueLiteralExpression
        ];

    public static bool AnyOfKind(this IEnumerable<SyntaxNode> nodes, SyntaxKind kind) =>
        nodes.Any(n => n.RawKind == (int)kind);

    public static bool AnyOfKind(this IEnumerable<SyntaxToken> tokens, SyntaxKind kind) =>
        tokens.Any(n => n.RawKind == (int)kind);

    public static SyntaxNode GetTopMostContainingMethod(this SyntaxNode node) =>
        node.AncestorsAndSelf().LastOrDefault(ancestor => ancestor is BaseMethodDeclarationSyntax || ancestor is PropertyDeclarationSyntax);

    public static SyntaxNode GetSelfOrTopParenthesizedExpression(this SyntaxNode node)
    {
        var current = node;
        while (current?.Parent?.IsKind(SyntaxKind.ParenthesizedExpression) ?? false)
        {
            current = current.Parent;
        }
        return current;
    }

    public static ExpressionSyntax GetSelfOrTopParenthesizedExpression(this ExpressionSyntax node) =>
         (ExpressionSyntax)GetSelfOrTopParenthesizedExpression((SyntaxNode)node);

    public static SyntaxNode GetFirstNonParenthesizedParent(this SyntaxNode node) =>
        node.GetSelfOrTopParenthesizedExpression().Parent;

    public static bool HasAncestorOfKind(this SyntaxNode syntaxNode, params SyntaxKind[] syntaxKinds) =>
        syntaxNode.Ancestors().Any(x => x.IsAnyKind(syntaxKinds));

    public static bool IsOnThis(this ExpressionSyntax expression) =>
        IsOn(expression, SyntaxKind.ThisExpression);

    private static bool IsOn(this ExpressionSyntax expression, SyntaxKind onKind) =>
        expression.Kind() switch
        {
            SyntaxKind.InvocationExpression => IsOn(((InvocationExpressionSyntax)expression).Expression, onKind),
            // Following statement is a simplification as we don't check where the method is defined (so this could be this or base)
            SyntaxKind.AliasQualifiedName or SyntaxKind.GenericName or SyntaxKind.IdentifierName or SyntaxKind.QualifiedName => true,
            SyntaxKind.PointerMemberAccessExpression or SyntaxKind.SimpleMemberAccessExpression => ((MemberAccessExpressionSyntax)expression).Expression.RemoveParentheses().IsKind(onKind),
            SyntaxKind.ConditionalAccessExpression => ((ConditionalAccessExpressionSyntax)expression).Expression.RemoveParentheses().IsKind(onKind),
            _ => false,
        };

    public static bool IsInNameOfArgument(this ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var parentInvocation = expression.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        return parentInvocation != null && parentInvocation.IsNameof(semanticModel);
    }

    public static bool IsNameof(this InvocationExpressionSyntax expression, SemanticModel semanticModel) =>
        expression != null &&
        expression.Expression is IdentifierNameSyntax identifierNameSyntax &&
        identifierNameSyntax.Identifier.ValueText == NameOfKeywordText &&
        semanticModel.GetSymbolInfo(expression).Symbol?.Kind != SymbolKind.Method;

    public static bool IsStringEmpty(this ExpressionSyntax expression, SemanticModel semanticModel)
    {
        if (!expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
            !expression.IsKind(SyntaxKind.PointerMemberAccessExpression))
        {
            return false;
        }

        var nameSymbolInfo = semanticModel.GetSymbolInfo(((MemberAccessExpressionSyntax)expression).Name);

        return nameSymbolInfo.Symbol != null &&
               nameSymbolInfo.Symbol.IsInType(KnownType.System_String) &&
               nameSymbolInfo.Symbol.Name == nameof(string.Empty);
    }

    public static bool IsNullLiteral(this SyntaxNode syntaxNode) =>
        syntaxNode != null && syntaxNode.IsKind(SyntaxKind.NullLiteralExpression);

    public static bool IsAnyKind(this SyntaxNode syntaxNode, params SyntaxKind[] syntaxKinds) =>
        syntaxNode != null && syntaxKinds.Contains((SyntaxKind)syntaxNode.RawKind);

    public static bool IsAnyKind(this SyntaxNode syntaxNode, ISet<SyntaxKind> syntaxKinds) =>
        syntaxNode != null && syntaxKinds.Contains((SyntaxKind)syntaxNode.RawKind);

    public static bool IsAnyKind(this SyntaxToken syntaxToken, params SyntaxKind[] syntaxKinds) =>
        syntaxKinds.Contains((SyntaxKind)syntaxToken.RawKind);

    public static bool IsAnyKind(this SyntaxToken syntaxToken, ISet<SyntaxKind> syntaxKinds) =>
        syntaxKinds.Contains((SyntaxKind)syntaxToken.RawKind);

    public static bool IsAnyKind(this SyntaxTrivia syntaxTravia, params SyntaxKind[] syntaxKinds) =>
        syntaxKinds.Contains((SyntaxKind)syntaxTravia.RawKind);

    public static bool ContainsMethodInvocation(this BaseMethodDeclarationSyntax methodDeclarationBase,
        SemanticModel semanticModel,
        Func<InvocationExpressionSyntax, bool> syntaxPredicate, Func<IMethodSymbol, bool> symbolPredicate)
    {
        var childNodes = methodDeclarationBase?.Body?.DescendantNodes()
            ?? methodDeclarationBase?.ExpressionBody()?.DescendantNodes()
            ?? Enumerable.Empty<SyntaxNode>();

        // See issue: https://github.com/SonarSource/sonar-dotnet/issues/416
        // Where clause excludes nodes that are not defined on the same SyntaxTree as the SemanticModel
        // (because of partial definition).
        // More details: https://github.com/dotnet/roslyn/issues/18730
        return childNodes
            .OfType<InvocationExpressionSyntax>()
            .Where(syntaxPredicate)
            .Select(e => e.Expression.SyntaxTree.GetSemanticModelOrDefault(semanticModel)?.GetSymbolInfo(e.Expression).Symbol)
            .OfType<IMethodSymbol>()
            .Any(symbolPredicate);
    }

    public static SyntaxToken? GetIdentifierOrDefault(this BaseMethodDeclarationSyntax methodDeclaration)
    {
        switch (methodDeclaration?.Kind())
        {
            case SyntaxKind.ConstructorDeclaration:
                return ((ConstructorDeclarationSyntax)methodDeclaration).Identifier;

            case SyntaxKind.DestructorDeclaration:
                return ((DestructorDeclarationSyntax)methodDeclaration).Identifier;

            case SyntaxKind.MethodDeclaration:
                return ((MethodDeclarationSyntax)methodDeclaration).Identifier;

            default:
                return null;
        }
    }

    public static bool IsMethodInvocation(this InvocationExpressionSyntax invocation, KnownType type, string methodName, SemanticModel semanticModel) =>
        invocation.Expression.NameIs(methodName) &&
        semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
        methodSymbol.IsInType(type);

    public static bool IsMethodInvocation(this InvocationExpressionSyntax invocation, ImmutableArray<KnownType> types, string methodName, SemanticModel semanticModel) =>
        invocation.Expression.NameIs(methodName) &&
        semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
        methodSymbol.IsInType(types);

    public static bool IsPropertyInvocation(this MemberAccessExpressionSyntax expression, ImmutableArray<KnownType> types, string propertyName, SemanticModel semanticModel) =>
        expression.NameIs(propertyName) &&
        semanticModel.GetSymbolInfo(expression).Symbol is IPropertySymbol propertySymbol &&
        propertySymbol.IsInType(types);

    public static Location FindIdentifierLocation(this BaseMethodDeclarationSyntax methodDeclaration) =>
        GetIdentifierOrDefault(methodDeclaration)?.GetLocation();

    public static bool HasDefaultLabel(this SwitchStatementSyntax node) =>
        GetDefaultLabelSectionIndex(node) >= 0;

    public static int GetDefaultLabelSectionIndex(this SwitchStatementSyntax node) =>
        node.Sections.IndexOf(section => section.Labels.AnyOfKind(SyntaxKind.DefaultSwitchLabel));

    public static bool HasBodyOrExpressionBody(this AccessorDeclarationSyntax node) =>
        node.Body != null || node.ExpressionBody() != null;

    public static string GetName(this SyntaxNode node) =>
        node.GetIdentifier()?.ValueText ?? string.Empty;

    public static bool NameIs(this SyntaxNode node, string name) =>
        node.GetName().Equals(name, StringComparison.Ordinal);

    public static bool NameIs(this SyntaxNode node, string name, params string[] orNames) =>
        node.GetName() is { } nodeName
        && (nodeName.Equals(name, StringComparison.Ordinal)
            || Array.Exists(orNames, x => nodeName.Equals(x, StringComparison.Ordinal)));

    public static bool HasConstantValue(this ExpressionSyntax expression, SemanticModel semanticModel) =>
        expression.RemoveParentheses().IsAnyKind(LiteralSyntaxKinds) || expression.FindConstantValue(semanticModel) != null;

    public static string StringValue(this SyntaxNode node, SemanticModel semanticModel) =>
        node switch
        {
            LiteralExpressionSyntax literal when literal.IsAnyKind(SyntaxKind.StringLiteralExpression, SyntaxKindEx.Utf8StringLiteralExpression) => literal.Token.ValueText,
            InterpolatedStringExpressionSyntax expression => expression.TryGetInterpolatedTextValue(semanticModel, out var interpolatedValue) ? interpolatedValue : expression.GetContentsText(),
            _ => null
        };

    public static bool IsLeftSideOfAssignment(this ExpressionSyntax expression)
    {
        var topParenthesizedExpression = expression.GetSelfOrTopParenthesizedExpression();
        return topParenthesizedExpression.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression)
               && topParenthesizedExpression.Parent is AssignmentExpressionSyntax assignment
               && assignment.Left == topParenthesizedExpression;
    }

    public static bool IsComment(this SyntaxTrivia trivia)
    {
        switch (trivia.Kind())
        {
            case SyntaxKind.SingleLineCommentTrivia:
            case SyntaxKind.MultiLineCommentTrivia:
            case SyntaxKind.SingleLineDocumentationCommentTrivia:
            case SyntaxKind.MultiLineDocumentationCommentTrivia:
                return true;

            default:
                return false;
        }
    }

    // creates a QualifiedNameSyntax "a.b"
    public static QualifiedNameSyntax BuildQualifiedNameSyntax(string a, string b) =>
        SyntaxFactory.QualifiedName(
            SyntaxFactory.IdentifierName(a),
            SyntaxFactory.IdentifierName(b));

    // creates a QualifiedNameSyntax "a.b.c"
    public static QualifiedNameSyntax BuildQualifiedNameSyntax(string a, string b, string c) =>
        SyntaxFactory.QualifiedName(
            SyntaxFactory.QualifiedName(
                SyntaxFactory.IdentifierName(a),
                SyntaxFactory.IdentifierName(b)),
            SyntaxFactory.IdentifierName(c));

    /// <summary>
    /// Returns argument expressions for given parameter.
    ///
    /// There can be zero, one or more results based on parameter type (Optional or ParamArray/params).
    /// </summary>
    public static ImmutableArray<SyntaxNode> ArgumentValuesForParameter(SemanticModel semanticModel, ArgumentListSyntax argumentList, string parameterName) =>
        argumentList != null
            && new CSharpMethodParameterLookup(argumentList, semanticModel).TryGetSyntax(parameterName, out var expressions)
                ? expressions
                : ImmutableArray<SyntaxNode>.Empty;
}
