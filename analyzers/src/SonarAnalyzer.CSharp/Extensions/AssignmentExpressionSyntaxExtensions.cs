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

using System;
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

        public static SyntaxNode FindTupleArgumentComplement(this SyntaxNode argument)
        {
            var outterTuple = argument.Ancestors()
                .TakeWhile(x => x.IsAnyKind(
                    SyntaxKindEx.TupleExpression,
                    SyntaxKind.Argument,
                    SyntaxKindEx.SingleVariableDesignation,
                    SyntaxKindEx.ParenthesizedVariableDesignation,
                    SyntaxKindEx.DiscardDesignation,
                    SyntaxKindEx.DeclarationExpression))
                .LastOrDefault();
            if ((TupleExpressionSyntaxWrapper.IsInstance(outterTuple) || DeclarationExpressionSyntaxWrapper.IsInstance(outterTuple))
                && outterTuple.Parent is AssignmentExpressionSyntax assignment)
            {
                var otherSide = assignment switch
                {
                    { Left: { } left, Right: { } right } when left.Equals(outterTuple) => right,
                    { Left: { } left, Right: { } right } when right.Equals(outterTuple) => left,
                    _ => null,
                };
                if (TupleExpressionSyntaxWrapper.IsInstance(otherSide) || DeclarationExpressionSyntaxWrapper.IsInstance(otherSide))
                {
                    var indexAndCount = outterTuple switch
                    {
                        _ when TupleExpressionSyntaxWrapper.IsInstance(outterTuple) => IndexAndCountOfTupleNesting(argument),
                        _ when DeclarationExpressionSyntaxWrapper.IsInstance(outterTuple) => IndexAndCountOfDeclarationNesting(argument),
                        _ => throw new InvalidOperationException("Unreachable"),
                    };
                    if (TupleExpressionSyntaxWrapper.IsInstance(otherSide) && (TupleExpressionSyntaxWrapper)otherSide is { } otherTuple)
                    {
                        return FindMatchingTupleArgument(indexAndCount, otherTuple);
                    }
                    else if (DeclarationExpressionSyntaxWrapper.IsInstance(otherSide) && (DeclarationExpressionSyntaxWrapper)otherSide is { } otherDesignation)
                    {
                        return FindMatchingDesignationElement(indexAndCount, otherDesignation);
                    }
                }
            }

            return null;

            static Stack<IndexCountPair> IndexAndCountOfTupleNesting(SyntaxNode argument)
            {
                Stack<IndexCountPair> indexAndCount = new();
                while (TupleExpressionSyntaxWrapper.IsInstance(argument?.Parent))
                {
                    var parentTuple = (TupleExpressionSyntaxWrapper)argument.Parent;
                    indexAndCount.Push(new(parentTuple.Arguments.IndexOf((ArgumentSyntax)argument), parentTuple.Arguments.Count));
                    argument = parentTuple.SyntaxNode.Parent as ArgumentSyntax;
                }

                return indexAndCount;
            }

            static Stack<IndexCountPair> IndexAndCountOfDeclarationNesting(SyntaxNode variable)
            {
                Stack<IndexCountPair> indexAndCount = new();
                while (ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(variable?.Parent))
                {
                    var parentdesignation = (ParenthesizedVariableDesignationSyntaxWrapper)variable.Parent;
                    indexAndCount.Push(new(parentdesignation.Variables.IndexOf((VariableDesignationSyntaxWrapper)variable), parentdesignation.Variables.Count));
                    variable = parentdesignation.SyntaxNode;
                }

                return indexAndCount;
            }

            static SyntaxNode FindMatchingTupleArgument(Stack<IndexCountPair> indexAndCount, TupleExpressionSyntaxWrapper tuple)
            {
                SyntaxNode argumentExpression = null;
                while (indexAndCount.Count > 0)
                {
                    var currentIndex = indexAndCount.Pop();
                    var expectedIndex = currentIndex.Item1;
                    var expectedCount = currentIndex.Item2;
                    if (tuple.Arguments.Count == expectedCount)
                    {
                        argumentExpression = tuple.Arguments[expectedIndex].Expression;
                    }
                    else
                    {
                        return null;
                    }
                    if (indexAndCount.Count > 0)
                    {
                        if (TupleExpressionSyntaxWrapper.IsInstance(argumentExpression))
                        {
                            tuple = (TupleExpressionSyntaxWrapper)argumentExpression;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                return argumentExpression;
            }

            static SyntaxNode FindMatchingDesignationElement(Stack<IndexCountPair> indexAndCount, DeclarationExpressionSyntaxWrapper declaration)
            {
                SyntaxNode designation = null;
                if (ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(declaration.Designation) && (ParenthesizedVariableDesignationSyntaxWrapper)declaration.Designation is { } designations)
                {
                    while (indexAndCount.Count > 0)
                    {
                        var currentIndex = indexAndCount.Pop();
                        var expectedIndex = currentIndex.Item1;
                        var expectedCount = currentIndex.Item2;
                        if (designations.Variables.Count == expectedCount)
                        {
                            designation = designations.Variables[expectedIndex];
                        }
                        else
                        {
                            return null;
                        }
                        if (indexAndCount.Count > 0)
                        {
                            if (ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(designation))
                            {
                                designations = (ParenthesizedVariableDesignationSyntaxWrapper)designation;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
                return designation;
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
