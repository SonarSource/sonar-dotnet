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

public static class SyntaxNodeExtensionsShared
{
    public static bool ContainsGetOrSetOnDependencyProperty(this SyntaxNode node, Compilation compilation)
    {
        var model = compilation.GetSemanticModel(node.SyntaxTree);
        // Ignore the accessor if it calls System.Windows.DependencyObject.GetValue or System.Windows.DependencyObject.SetValue
        return node
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(x => x.Expression.NameIs("GetValue") || x.Expression.NameIs("SetValue"))
            .Any(x => model.GetSymbolInfo(x).Symbol.ContainingType.DerivesFrom(KnownType.System_Windows_DependencyObject));
    }

    public static IEnumerable<StatementSyntax> GetPreviousStatementsCurrentBlock(this SyntaxNode expression)
    {
        var statement = expression.FirstAncestorOrSelf<StatementSyntax>();
        return statement is null ? [] : statement.Parent.ChildNodes().OfType<StatementSyntax>().TakeWhile(x => x != statement).Reverse();
    }
}
