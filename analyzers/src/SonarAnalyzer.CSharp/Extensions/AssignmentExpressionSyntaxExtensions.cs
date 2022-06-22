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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.Extensions
{
    internal static class AssignmentExpressionSyntaxExtensions
    {
        public readonly record struct AssignmentMapping(CSharpSyntaxNode Left, ExpressionSyntax Right);

        /// <summary>
        /// Maps the left and the right side arguments of an <paramref name="assignment"/>. If both sides are tuples, the tuple elements are mapped.
        /// <code>
        /// (var x, var y) = (1, 2);                 // [x→1, y→2]
        /// (var x, (var y, var z)) = (1, (2, 3));   // [x→1, y→2, z→3]
        /// var x = 1;                               // [x→1]
        /// (var x, var y) = M();                    // [(x,y)→M()]
        /// </code>
        /// </summary>
        /// <param name="assignment">The <paramref name="assignment"/> expression.</param>
        /// <returns>A mapping from expressions on the left side of the <paramref name="assignment"/> to the right side.</returns>
        public static ImmutableArray<AssignmentMapping> MapAssignmentArguments(this AssignmentExpressionSyntax assignment)
        {
            // (var x, var y) = (1, 2)
            if (TupleExpressionSyntaxWrapper.IsInstance(assignment.Left)
                && TupleExpressionSyntaxWrapper.IsInstance(assignment.Right))
            {
                var left = (TupleExpressionSyntaxWrapper)assignment.Left;
                var right = (TupleExpressionSyntaxWrapper)assignment.Right;
                var arrayBuilder = ImmutableArray.CreateBuilder<AssignmentMapping>(left.Arguments.Count);
                MapTupleElements(arrayBuilder, left, right);
                return arrayBuilder.ToImmutableArray();
            }
            // var (x, y) = (1, 2)
            else if (DeclarationExpressionSyntaxWrapper.IsInstance(assignment.Left)
                    && (DeclarationExpressionSyntaxWrapper)assignment.Left is { Designation: { } leftDesignation }
                    && ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(leftDesignation)
                    && TupleExpressionSyntaxWrapper.IsInstance(assignment.Right))
            {
                var left = (ParenthesizedVariableDesignationSyntaxWrapper)leftDesignation;
                var right = (TupleExpressionSyntaxWrapper)assignment.Right;
                var arrayBuilder = ImmutableArray.CreateBuilder<AssignmentMapping>(right.Arguments.Count);
                MapDesignationElements(arrayBuilder, left, right);
                return arrayBuilder.ToImmutableArray();
            }
            else
            {
                return ImmutableArray.Create(new AssignmentMapping(assignment.Left, assignment.Right));
            }

            static void MapTupleElements(ImmutableArray<AssignmentMapping>.Builder arrayBuilder, TupleExpressionSyntaxWrapper left, TupleExpressionSyntaxWrapper right)
            {
                var leftEnum = left.Arguments.GetEnumerator();
                var rightEnum = right.Arguments.GetEnumerator();
                while (leftEnum.MoveNext() && rightEnum.MoveNext())
                {
                    var leftArg = leftEnum.Current;
                    var rightArg = rightEnum.Current;
                    if (leftArg is { Expression: { } leftExpression }
                        && TupleExpressionSyntaxWrapper.IsInstance(leftExpression)
                        && rightArg is { Expression: { } rightExpression }
                        && TupleExpressionSyntaxWrapper.IsInstance(rightExpression))
                    {
                        MapTupleElements(arrayBuilder, (TupleExpressionSyntaxWrapper)leftExpression, (TupleExpressionSyntaxWrapper)rightExpression);
                    }
                    else
                    {
                        arrayBuilder.Add(new AssignmentMapping(leftArg.Expression, rightArg.Expression));
                    }
                }
            }

            static void MapDesignationElements(ImmutableArray<AssignmentMapping>.Builder arrayBuilder, ParenthesizedVariableDesignationSyntaxWrapper left, TupleExpressionSyntaxWrapper right)
            {
                var leftEnum = left.Variables.GetEnumerator();
                var rightEnum = right.Arguments.GetEnumerator();
                while (leftEnum.MoveNext() && rightEnum.MoveNext())
                {
                    var leftVar = leftEnum.Current;
                    var rightArg = rightEnum.Current;
                    if (ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(leftVar)
                        && rightArg is { Expression: { } rightExpression }
                        && TupleExpressionSyntaxWrapper.IsInstance(rightExpression))
                    {
                        MapDesignationElements(arrayBuilder, (ParenthesizedVariableDesignationSyntaxWrapper)leftVar, (TupleExpressionSyntaxWrapper)rightExpression);
                    }
                    else
                    {
                        arrayBuilder.Add(new AssignmentMapping(leftVar.SyntaxNode, rightArg.Expression));
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of nodes, that represent the target (left side) of an assignment. In case of tuple deconstructions, this can be more than one target.
        /// Nested tuple elements are flattened so for <c>(a, (b, c))</c> the list <c>[a, b, c]</c> is returned.
        /// </summary>
        /// <param name="assignment">The assignment expression.</param>
        /// <returns>The left side of the assignment. If it is a tuple, the flattened tuple elements are returned.</returns>
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
