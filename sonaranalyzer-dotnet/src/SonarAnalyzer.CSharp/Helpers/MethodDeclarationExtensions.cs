/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
            syntaxNode.DescendantNodes().OfType<ThrowStatementSyntax>().Any() ||
            syntaxNode.DescendantNodes().OfType<ExpressionSyntax>().Any(expression => expression.IsKind(SyntaxKindEx.ThrowExpression)) ||
            syntaxNode.DescendantNodes().OfType<ReturnStatementSyntax>().Any(returnStatement => returnStatement.Expression.IsKind(SyntaxKind.NullLiteralExpression)) ||
            // For simplicity this returns true for any method witch contains a NullLiteralExpression but this could be a source of FNs
            syntaxNode.DescendantNodes().OfType<ExpressionSyntax>().Any(expression => expression.IsKind(SyntaxKind.NullLiteralExpression));
    }
}
