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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;
using IndexCountPair = System.Tuple<int, int>;

namespace SonarAnalyzer.Extensions
{
    internal static class AssignmentExpressionSyntaxExtensions
    {
        public readonly record struct AssignmentMapping(SyntaxNode Left, SyntaxNode Right);

        /// <summary>
        /// Maps the left and the right side arguments of an <paramref name="assignment"/>. If both sides are tuples, the tuple elements are mapped.
        /// <code>
        /// (var x, var y) = (1, 2);                 // [x←1, y←2]
        /// var (x, y) = (1, 2);                     // [x←1, y←2]
        /// (var x, (var y, var z)) = (1, (2, 3));   // [x←1, y←2, z←3]
        /// var x = 1;                               // [x←1]
        /// (var x, var y) = M();                    // [(x,y)←M()]
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

        /// <summary>
        /// Returns a list of nodes, that represent the target (left side) of an assignment. In case of tuple deconstructions, this can be more than one target.
        /// Nested tuple elements are flattened so for <c>(a, (b, c))</c> the list <c>[a, b, c]</c> is returned.
        /// </summary>
        public static ImmutableArray<SyntaxNode> AssignmentTargets(this AssignmentExpressionSyntax assignment)
        {
            var left = assignment.Left;
            if (TupleExpressionSyntaxWrapper.IsInstance(left))
            {
                var tuple = (TupleExpressionSyntaxWrapper)left;
                var argumentExpressions = tuple.AllArguments().Select(x => (SyntaxNode)x.Expression);
                var designationsExpanded = argumentExpressions.SelectMany(x => x.IsKind(SyntaxKindEx.DeclarationExpression)
                    ? ((DeclarationExpressionSyntaxWrapper)x).Designation.AllVariables().Select(v => (SyntaxNode)v)
                    : Enumerable.Repeat(x, 1));
                return designationsExpanded.ToImmutableArray();
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

        /// <summary>
        /// Finds the complementing <see cref="SyntaxNode"/> of an assignment with tuples. If <paramref name="node"/> is <c>x</c>, the method returns <c>r</c>. In the following examples:
        /// <code>
        /// var (a, x) = (1, r);
        /// var (a, r) = (1, x);
        /// (var a, var x) = (1, r);
        /// (var a, var r) = (1, x);
        /// </code>
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static SyntaxNode FindAssignmentTupleComplement(this SyntaxNode node)
        {
            var thisSide = node.Ancestors()
                .TakeWhile(x => x.IsAnyKind(
                    SyntaxKindEx.TupleExpression,
                    SyntaxKind.Argument,
                    SyntaxKindEx.SingleVariableDesignation,
                    SyntaxKindEx.ParenthesizedVariableDesignation,
                    SyntaxKindEx.DiscardDesignation,
                    SyntaxKindEx.DeclarationExpression))
                .LastOrDefault();
            if ((TupleExpressionSyntaxWrapper.IsInstance(thisSide) || DeclarationExpressionSyntaxWrapper.IsInstance(thisSide))
                && thisSide.Parent is AssignmentExpressionSyntax assignment)
            {
                var otherSide = assignment switch
                {
                    { Left: { } left, Right: { } right } when left.Equals(thisSide) => right,
                    { Left: { } left, Right: { } right } when right.Equals(thisSide) => left,
                    _ => null,
                };
                if (TupleExpressionSyntaxWrapper.IsInstance(otherSide) || DeclarationExpressionSyntaxWrapper.IsInstance(otherSide))
                {
                    var indexAndCount = IndexAndCountOfNesting(node);
                    return FindMatchingNestedNode(indexAndCount, otherSide);
                }
            }

            return null;

            static Stack<IndexCountPair> IndexAndCountOfNesting(SyntaxNode node)
            {
                Stack<IndexCountPair> indexAndCount = new();
                while (TupleExpressionSyntaxWrapper.IsInstance(node?.Parent) || ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(node?.Parent))
                {
                    if (TupleExpressionSyntaxWrapper.IsInstance(node.Parent))
                    {
                        var parentTuple = (TupleExpressionSyntaxWrapper)node.Parent;
                        indexAndCount.Push(new(parentTuple.Arguments.IndexOf((ArgumentSyntax)node), parentTuple.Arguments.Count));
                        node = parentTuple.SyntaxNode.Parent;
                    }
                    else if (ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(node.Parent))
                    {
                        var parentdesignation = (ParenthesizedVariableDesignationSyntaxWrapper)node.Parent;
                        indexAndCount.Push(new(parentdesignation.Variables.IndexOf((VariableDesignationSyntaxWrapper)node), parentdesignation.Variables.Count));
                        node = parentdesignation.SyntaxNode;
                    }
                    if (DeclarationExpressionSyntaxWrapper.IsInstance(node.Parent)
                        && node.Parent.Parent is ArgumentSyntax)
                    {
                        node = node.Parent?.Parent;
                    }
                }
                return indexAndCount;
            }

            static SyntaxNode FindMatchingNestedNode(Stack<IndexCountPair> indexAndCount, SyntaxNode node)
            {
                SyntaxNode argumentExpression = null;
                while (indexAndCount.Count > 0)
                {
                    if (DeclarationExpressionSyntaxWrapper.IsInstance(node))
                    {
                        node = ((DeclarationExpressionSyntaxWrapper)node).Designation;
                    }
                    var currentIndex = indexAndCount.Pop();
                    var expectedIndex = currentIndex.Item1;
                    var expectedCount = currentIndex.Item2;
                    if (TupleExpressionSyntaxWrapper.IsInstance(node))
                    {
                        var tuple = (TupleExpressionSyntaxWrapper)node;
                        if (tuple.Arguments.Count == expectedCount)
                        {
                            argumentExpression = tuple.Arguments[expectedIndex].Expression;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(node))
                    {
                        var parenthesizedDesignation = (ParenthesizedVariableDesignationSyntaxWrapper)node;
                        if (parenthesizedDesignation.Variables.Count == expectedCount)
                        {
                            argumentExpression = parenthesizedDesignation.Variables[expectedIndex];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                    node = argumentExpression;
                }
                return argumentExpression;
            }
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
                else if (DeclarationExpressionSyntaxWrapper.IsInstance(leftExpression)
                     && (DeclarationExpressionSyntaxWrapper)leftExpression is { Designation: { } leftDesignation }
                     && ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(leftDesignation)
                     && TupleExpressionSyntaxWrapper.IsInstance(rightExpression))
                {
                    if (!MapDesignationElements(arrayBuilder, (ParenthesizedVariableDesignationSyntaxWrapper)leftDesignation, (TupleExpressionSyntaxWrapper)rightExpression))
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
    }
}
