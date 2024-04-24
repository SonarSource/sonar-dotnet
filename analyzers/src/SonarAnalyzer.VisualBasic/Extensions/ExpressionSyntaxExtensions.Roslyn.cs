// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.VisualBasic.Extensions;

[ExcludeFromCodeCoverage]
internal static class ExpressionSyntaxExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="expression"/> represents a node where a value is written to, like on the left side of an assignment expression. This method
    /// also returns <see langword="true"/> for potentially writeable expressions, like <see langword="ref"/> parameters.
    /// See also <seealso cref="IsOnlyWrittenTo(ExpressionSyntax)"/>.
    /// </summary>
    /// <remarks>
    /// Copied from <seealso href="https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/VisualBasic/Extensions/ExpressionSyntaxExtensions.vb#L362"/>
    /// </remarks>
    public static bool IsWrittenTo(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (expression == null)
            return false;

        if (expression.IsOnlyWrittenTo())
            return true;

        if (expression.IsRightSideOfDot())
            expression = expression.Parent as ExpressionSyntax;

        if (expression != null)
        {
            if (expression.IsInRefContext(semanticModel, cancellationToken))
                return true;

            if (expression.Parent is AssignmentStatementSyntax)
            {
                var assignmentStatement = (AssignmentStatementSyntax)expression.Parent;
                if (expression == assignmentStatement.Left)
                    return true;
            }

            if (expression.IsChildNode<NamedFieldInitializerSyntax>(n => n.Name))
                return true;

            // Extension method with a 'ref' parameter can write to the value it is called on.
            if (expression.Parent is MemberAccessExpressionSyntax)
            {
                var memberAccess = (MemberAccessExpressionSyntax)expression.Parent;
                if (memberAccess.Expression == expression)
                {
                    var method = semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol as IMethodSymbol;
                    if (method != null)
                    {
                        if (method.MethodKind == MethodKind.ReducedExtension && method.ReducedFrom.Parameters.Length > 0 && method.ReducedFrom.Parameters.First().RefKind == RefKind.Ref)
                            return true;
                    }
                }
            }

            return false;
        }

        return false;
    }

    // Copy of
    // https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/VisualBasic/Extensions/ExpressionSyntaxExtensions.vb#L325
    public static bool IsOnlyWrittenTo(this ExpressionSyntax expression)
    {
        if (expression.IsRightSideOfDot())
            expression = expression.Parent as ExpressionSyntax;

        if (expression != null)
        {
            // Sonar: IsInOutContext deleted because not relevant for VB
            if (expression.IsParentKind(SyntaxKind.SimpleAssignmentStatement))
            {
                var assignmentStatement = (AssignmentStatementSyntax)expression.Parent;
                if (expression == assignmentStatement.Left)
                    return true;
            }

            if (expression.IsParentKind(SyntaxKind.NameColonEquals) && expression.Parent.IsParentKind(SyntaxKind.SimpleArgument))

                // <C(Prop:=1)>
                // this is only a write to Prop
                return true;

            if (expression.IsChildNode<NamedFieldInitializerSyntax>(n => n.Name))
                return true;

            return false;
        }

        return false;
    }

    // Copy of
    // https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/VisualBasic/Extensions/ExpressionSyntaxExtensions.vb#L73
    public static bool IsRightSideOfDot(this ExpressionSyntax expression)
    {
        return expression.IsSimpleMemberAccessExpressionName() || expression.IsRightSideOfQualifiedName();
    }

    // Copy of
    // https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/VisualBasic/Extensions/ExpressionSyntaxExtensions.vb#L56
    public static bool IsSimpleMemberAccessExpressionName(this ExpressionSyntax expression)
    {
        return expression.IsParentKind(SyntaxKind.SimpleMemberAccessExpression) && ((MemberAccessExpressionSyntax)expression.Parent).Name == expression;
    }

    // Copy of
    // https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/VisualBasic/Extensions/ExpressionSyntaxExtensions.vb#L78
    public static bool IsRightSideOfQualifiedName(this ExpressionSyntax expression)
    {
        return expression.IsParentKind(SyntaxKind.QualifiedName) && ((QualifiedNameSyntax)expression.Parent).Right == expression;
    }

    // Copy of
    // https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/VisualBasic/Extensions/ExpressionSyntaxExtensions.vb#L277
    public static bool IsInRefContext(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var simpleArgument = expression?.Parent as SimpleArgumentSyntax;

        if (simpleArgument == null)
            return false;
        else if (simpleArgument.IsNamed)
        {
            var info = semanticModel.GetSymbolInfo(simpleArgument.NameColonEquals.Name, cancellationToken);

            var parameter = info.Symbol as IParameterSymbol;
            return parameter != null && parameter.RefKind != RefKind.None;
        }
        else
        {
            var argumentList = simpleArgument.Parent as ArgumentListSyntax;

            if (argumentList != null)
            {
                var parent = argumentList.Parent;
                var index = argumentList.Arguments.IndexOf(simpleArgument);

                var info = semanticModel.GetSymbolInfo(parent, cancellationToken);
                var symbol = info.Symbol;

                if (symbol is IMethodSymbol)
                {
                    var method = (IMethodSymbol)symbol;
                    if (index < method.Parameters.Length)
                        return method.Parameters[index].RefKind != RefKind.None;
                }
                else if (symbol is IPropertySymbol)
                {
                    var prop = (IPropertySymbol)symbol;
                    if (index < prop.Parameters.Length)
                        return prop.Parameters[index].RefKind != RefKind.None;
                }
            }
        }

        return false;
    }
}
