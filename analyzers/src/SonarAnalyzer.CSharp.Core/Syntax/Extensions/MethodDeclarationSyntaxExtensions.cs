/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class MethodDeclarationSyntaxExtensions
{
    /// <summary>
    /// Returns true if the method throws exceptions or returns null.
    /// </summary>
    public static bool ThrowsOrReturnsNull(this MethodDeclarationSyntax syntaxNode) =>
        syntaxNode.DescendantNodes().OfType<ThrowStatementSyntax>().Any() ||
        syntaxNode.DescendantNodes().OfType<ExpressionSyntax>().Any(expression => expression.IsKind(SyntaxKindEx.ThrowExpression)) ||
        syntaxNode.DescendantNodes().OfType<ReturnStatementSyntax>().Any(returnStatement => returnStatement.Expression.IsKind(SyntaxKind.NullLiteralExpression)) ||
        // For simplicity this returns true for any method witch contains a NullLiteralExpression but this could be a source of FNs
        syntaxNode.DescendantNodes().OfType<ExpressionSyntax>().Any(expression => expression.IsKind(SyntaxKind.NullLiteralExpression));

    public static bool IsExtensionMethod(this BaseMethodDeclarationSyntax methodDeclaration) =>
        methodDeclaration.ParameterList.Parameters.Count > 0
        && methodDeclaration.ParameterList.Parameters[0].Modifiers.Any(s => s.IsKind(SyntaxKind.ThisKeyword));

    public static bool HasReturnTypeVoid(this MethodDeclarationSyntax methodDeclaration) =>
        methodDeclaration.ReturnType is PredefinedTypeSyntax { Keyword: { RawKind: (int)SyntaxKind.VoidKeyword } };

    public static bool IsDeconstructor(this MethodDeclarationSyntax methodDeclaration)
    {
        return  methodDeclaration.HasReturnTypeVoid()
                && (methodDeclaration.IsExtensionMethod() || !methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                && methodDeclaration.Identifier.Value.Equals("Deconstruct")
                && AllParametersHaveModifierOut(methodDeclaration);

        static bool AllParametersHaveModifierOut(MethodDeclarationSyntax methodDeclaration) =>
            (methodDeclaration.IsExtensionMethod()
             ? methodDeclaration.ParameterList.Parameters.Skip(1)
             : methodDeclaration.ParameterList.Parameters)
            .All(x => x.Modifiers.Any(y => y.IsKind(SyntaxKind.OutKeyword)));
    }
}
