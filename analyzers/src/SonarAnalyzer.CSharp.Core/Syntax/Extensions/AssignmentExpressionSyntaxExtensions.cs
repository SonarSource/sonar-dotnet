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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class AssignmentExpressionSyntaxExtensions
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
            if (MapTupleElements(arrayBuilder, left, right) is NestingMatch.Handled)
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
            if (MapDesignationElements(arrayBuilder, left, right) is NestingMatch.Handled)
            {
                return arrayBuilder.ToImmutableArray();
            }
        }

        return ImmutableArray.Create(new AssignmentMapping(assignment.Left, assignment.Right));
    }

    /// <summary>
    /// Returns a list of nodes, that represent the target (left side) of an assignment. In case of tuple deconstructions, this can be more than one target.
    /// Nested tuple elements and any declaration expressions are flattened.
    /// <code>
    /// (var a, var b)        = x; // [a, b]
    /// var (a, b)            = x; // [a, b]
    /// var (a, _)            = x; // [a]       ← The _ here is always a discard
    /// (var a, _)            = x; // [a, _]    ← The _ here could reference a local
    /// (var a, var (b, c))   = x; // [a, b, c]
    /// (var a, (int, int) b) = x; // [a, b]
    /// </code>
    /// </summary>
    public static ImmutableArray<SyntaxNode> AssignmentTargets(this AssignmentExpressionSyntax assignment)
    {
        var left = assignment.Left;
        if (TupleExpressionSyntaxWrapper.IsInstance(left))
        {
            var tuple = (TupleExpressionSyntaxWrapper)left;
            var argumentExpressions = tuple.AllArguments().Select(x => (SyntaxNode)x.Expression);
            var designationsExpanded = argumentExpressions.SelectMany(x => x.IsKind(SyntaxKindEx.DeclarationExpression)
                    ? ((DeclarationExpressionSyntaxWrapper)x).Designation.AllVariables().Select(x => (SyntaxNode)x)
                    : new[] { x });
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

    private static NestingMatch MapTupleElements(ImmutableArray<AssignmentMapping>.Builder arrayBuilder, TupleExpressionSyntaxWrapper left, TupleExpressionSyntaxWrapper right)
    {
        if (left.Arguments.Count != right.Arguments.Count)
        {
            return NestingMatch.Failed;
        }

        var leftEnumerator = left.Arguments.GetEnumerator();
        var rightEnumerator = right.Arguments.GetEnumerator();
        while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
        {
            var leftArgumentExpression = leftEnumerator.Current.Expression;
            var rightArgumentExpression = rightEnumerator.Current.Expression;
            switch (HandleTupleNesting(arrayBuilder, leftArgumentExpression, rightArgumentExpression))
            {
                case NestingMatch.Handled:
                    break; // the switch
                case NestingMatch.Failed:
                    return NestingMatch.Failed;
                case NestingMatch.Leaf:
                    arrayBuilder.Add(new AssignmentMapping(leftArgumentExpression, rightArgumentExpression));
                    break; // the switch
            }
        }

        return NestingMatch.Handled;
    }

    private static NestingMatch MapDesignationElements(ImmutableArray<AssignmentMapping>.Builder arrayBuilder,
                                                       ParenthesizedVariableDesignationSyntaxWrapper left,
                                                       TupleExpressionSyntaxWrapper right)
    {
        if (left.Variables.Count != right.Arguments.Count)
        {
            return NestingMatch.Failed;
        }

        var leftEnumerator = left.Variables.GetEnumerator();
        var rightEnumerator = right.Arguments.GetEnumerator();
        while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
        {
            var leftVar = leftEnumerator.Current;
            var rightExpression = rightEnumerator.Current.Expression;
            switch (HandleDesignationNesting(arrayBuilder, leftVar, rightExpression))
            {
                case NestingMatch.Handled:
                    break; // the switch
                case NestingMatch.Failed:
                    return NestingMatch.Failed;
                case NestingMatch.Leaf:
                    arrayBuilder.Add(new AssignmentMapping(leftVar.SyntaxNode, rightExpression));
                    break; // the switch
            }
        }

        return NestingMatch.Handled;
    }

    private static NestingMatch HandleTupleNesting(ImmutableArray<AssignmentMapping>.Builder arrayBuilder, ExpressionSyntax leftExpression, ExpressionSyntax rightExpression) =>
        true switch
        {
            _ when TupleExpressionSyntaxWrapper.IsInstance(leftExpression) && TupleExpressionSyntaxWrapper.IsInstance(rightExpression) =>
                MapTupleElements(arrayBuilder, (TupleExpressionSyntaxWrapper)leftExpression, (TupleExpressionSyntaxWrapper)rightExpression),
            _ when DeclarationExpressionSyntaxWrapper.IsInstance(leftExpression)
                && (DeclarationExpressionSyntaxWrapper)leftExpression is { Designation: { } leftDesignation } =>
                HandleDesignationNesting(arrayBuilder, leftDesignation, rightExpression),
            _ => NestingMatch.Leaf,
        };

    private static NestingMatch HandleDesignationNesting(ImmutableArray<AssignmentMapping>.Builder arrayBuilder, VariableDesignationSyntaxWrapper leftVar, ExpressionSyntax rightExpression) =>
        true switch
        {
            _ when ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(leftVar) && TupleExpressionSyntaxWrapper.IsInstance(rightExpression) =>
                MapDesignationElements(arrayBuilder, (ParenthesizedVariableDesignationSyntaxWrapper)leftVar, (TupleExpressionSyntaxWrapper)rightExpression),
            _ => NestingMatch.Leaf,
        };

    private enum NestingMatch
    {
        Handled,
        Failed,
        Leaf
    }
}
