/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Extensions
{
    public static class AssignmentExpressionSyntaxExtensions
    {
        /// <summary>
        /// Returns a list of nodes, that represent the target (left side) of an assignment. In case of tuple deconstructions, this can be more than one target.
        /// Nested tuple elements are flattened so for <c>(a, (b, c))</c> the list <c>[a, b, c]</c> is returned.
        /// </summary>
        /// <param name="assignment">The assignment expression.</param>
        /// <returns>The left side of the assignment. If it is a tuple, the flattened tuple elements are returned.</returns>
        public static ImmutableArray<SyntaxNode> AssignmentTargets(this AssignmentExpressionSyntax assignment)
        {
            var left = assignment.Left;
            if (TupleExpressionSyntaxWrapper.IsInstance(left))
            {
                var tuple = (TupleExpressionSyntaxWrapper)left;
                return tuple.AllArguments().Select(x => (SyntaxNode)x.Expression).ToImmutableArray();
            }
            else if (DeclarationExpressionSyntaxWrapper.IsInstance(left))
            {
                var declaration = (DeclarationExpressionSyntaxWrapper)left;
                return declaration.Designation.AllVariables().Select(x => (SyntaxNode)x).ToImmutableArray();
            }
            else
            {
                return ImmutableArray.Create<SyntaxNode>(left);
            }
        }
    }
}
