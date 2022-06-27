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
    internal static class AssignmentExpressionSyntaxExtensions
    {
        #region MapAssignmentArguments

        public readonly record struct AssignmentMapping(SyntaxNode Left, SyntaxNode Right);

        /// <summary>
        /// Maps the left and the right side arguments of an <paramref name="assignment"/>. If both sides are tuples, the tuple elements are mapped.
        /// <code>
        /// (var x, var y) = (1, 2);                 // [x→1, y→2]
        /// var (x, y) = (1, 2);                     // [x→1, y→2]
        /// (var x, (var y, var z)) = (1, (2, 3));   // [x→1, y→2, z→3]
        /// var x = 1;                               // [x→1]
        /// (var x, var y) = M();                    // [(x,y)→M()]
        /// </code>
        /// </summary>
        public static ImmutableArray<AssignmentMapping> MapAssignmentArguments(this AssignmentExpressionSyntax assignment)
        {
            // (var x, var y) = (1, 2)
            if (TupleExpressionSyntaxWrapper.IsInstance(assignment.Left)
                && TupleExpressionSyntaxWrapper.IsInstance(assignment.Right))
            {
                var left = (TupleExpressionSyntaxWrapper)assignment.Left;
                var right = (TupleExpressionSyntaxWrapper)assignment.Right;
                var arrayBuilder = ImmutableArray.CreateBuilder<AssignmentMapping>(left.Arguments.Count);
                if (MapTupleElements(arrayBuilder, left, right))
                {
                    return arrayBuilder.ToImmutableArray();
                }
            }
            // var (x, y) = (1, 2)
            else if (DeclarationExpressionSyntaxWrapper.IsInstance(assignment.Left)
                     && (DeclarationExpressionSyntaxWrapper)assignment.Left is { Designation: { } leftDesignation }
                     && ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(leftDesignation)
                     && TupleExpressionSyntaxWrapper.IsInstance(assignment.Right))
            {
                var left = (ParenthesizedVariableDesignationSyntaxWrapper)leftDesignation;
                var right = (TupleExpressionSyntaxWrapper)assignment.Right;
                var arrayBuilder = ImmutableArray.CreateBuilder<AssignmentMapping>(left.Variables.Count);
                if (MapDesignationElements(arrayBuilder, left, right))
                {
                    return arrayBuilder.ToImmutableArray();
                }
            }

            return ImmutableArray.Create(new AssignmentMapping(assignment.Left, assignment.Right));
        }

        private static bool MapTupleElements(ImmutableArray<AssignmentMapping>.Builder arrayBuilder, TupleExpressionSyntaxWrapper left, TupleExpressionSyntaxWrapper right)
        {
            if (left.Arguments.Count != right.Arguments.Count)
            {
                return false;
            }

            var leftEnumerator = left.Arguments.GetEnumerator();
            var rightEnumerator = right.Arguments.GetEnumerator();
            while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
            {
                var leftExpression = leftEnumerator.Current.Expression;
                var rightExpression = rightEnumerator.Current.Expression;
                if (TupleExpressionSyntaxWrapper.IsInstance(leftExpression)
                    && TupleExpressionSyntaxWrapper.IsInstance(rightExpression))
                {
                    if (!MapTupleElements(arrayBuilder, (TupleExpressionSyntaxWrapper)leftExpression, (TupleExpressionSyntaxWrapper)rightExpression))
                    {
                        return false;
                    }
                }
                else
                {
                    arrayBuilder.Add(new AssignmentMapping(leftExpression, rightExpression));
                }
            }

            return true;
        }

        private static bool MapDesignationElements(ImmutableArray<AssignmentMapping>.Builder arrayBuilder, ParenthesizedVariableDesignationSyntaxWrapper left, TupleExpressionSyntaxWrapper right)
        {
            if (left.Variables.Count != right.Arguments.Count)
            {
                return false;
            }

            var leftEnumerator = left.Variables.GetEnumerator();
            var rightEnumerator = right.Arguments.GetEnumerator();
            while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
            {
                var leftVar = leftEnumerator.Current;
                var rightExpression = rightEnumerator.Current.Expression;
                if (ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(leftVar)
                    && TupleExpressionSyntaxWrapper.IsInstance(rightExpression))
                {
                    if (!MapDesignationElements(arrayBuilder, (ParenthesizedVariableDesignationSyntaxWrapper)leftVar, (TupleExpressionSyntaxWrapper)rightExpression))
                    {
                        return false;
                    }
                }
                else
                {
                    arrayBuilder.Add(new AssignmentMapping(leftVar.SyntaxNode, rightExpression));
                }
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Returns a list of nodes, that represent the target (left side) of an assignment. In case of tuple deconstructions, this can be more than one target.
        /// Nested tuple elements are flattened so for <c>(a, (b, c))</c> the list <c>[a, b, c]</c> is returned.
        /// </summary>
        public static ImmutableArray<CSharpSyntaxNode> AssignmentTargets(this AssignmentExpressionSyntax assignment)
        {
            var left = assignment.Left;
            if (TupleExpressionSyntaxWrapper.IsInstance(left))
            {
                var tuple = (TupleExpressionSyntaxWrapper)left;
                return tuple.AllArguments().Select(x => (CSharpSyntaxNode)x.Expression).ToImmutableArray();
            }
            else if (DeclarationExpressionSyntaxWrapper.IsInstance(left))
            {
                var declaration = (DeclarationExpressionSyntaxWrapper)left;
                return declaration.Designation.AllVariables().Select(x => x.SyntaxNode).ToImmutableArray();
            }
            else
            {
                return ImmutableArray.Create<CSharpSyntaxNode>(left);
            }
        }
    }
}
