/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using SonarAnalyzer.Helpers;

#if CS
using Microsoft.CodeAnalysis.CSharp.Syntax;
#else
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
#endif

namespace SonarAnalyzer.Extensions
{
    internal static partial class SyntaxNodeExtensions
    {
        public static bool ContainsGetOrSetOnDependencyProperty(this SyntaxNode syntaxNode, Compilation compilation)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxNode.SyntaxTree);

            // Ignore the accessor if it calls System.Windows.DependencyObject.GetValue or System.Windows.DependencyObject.SetValue
            return syntaxNode
                   .DescendantNodes()
                   .OfType<InvocationExpressionSyntax>()
                   .Where(invocation => invocation.Expression.NameIs("GetValue") || invocation.Expression.NameIs("SetValue"))
                   .Any(invocation => semanticModel.GetSymbolInfo(invocation).Symbol.ContainingType.DerivesFrom(KnownType.System_Windows_DependencyObject));
        }
    }
}
