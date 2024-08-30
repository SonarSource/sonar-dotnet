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

namespace SonarAnalyzer.Extensions;

internal static class BaseMethodDeclarationSyntaxExtensions
{
    public static IEnumerable<SyntaxNode> GetBodyDescendantNodes(this BaseMethodDeclarationSyntax method) =>
        (method ?? throw new ArgumentNullException(nameof(method))).Body == null
            ? method.ExpressionBody().DescendantNodes()
            : method.Body.DescendantNodes();

    public static bool IsStatic(this BaseMethodDeclarationSyntax methodDeclaration) =>
        methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);

    public static bool IsExtern(this BaseMethodDeclarationSyntax methodDeclaration) =>
        methodDeclaration.Modifiers.Any(SyntaxKind.ExternKeyword);

    public static bool HasBodyOrExpressionBody(this BaseMethodDeclarationSyntax node) =>
        node.GetBodyOrExpressionBody() is not null;

    public static SyntaxNode GetBodyOrExpressionBody(this BaseMethodDeclarationSyntax node) =>
        (node?.Body as SyntaxNode) ?? node?.ExpressionBody()?.Expression;
}
