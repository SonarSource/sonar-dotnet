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
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Extensions
{
    internal static partial class SyntaxNodeExtensions
    {
        private static readonly ControlFlowGraphCache CfgCache = new();

        public static ControlFlowGraph CreateCfg(this SyntaxNode body, SemanticModel model) =>
            CfgCache.FindOrCreate(body.Parent, model);

        public static bool ContainsConditionalConstructs(this SyntaxNode node) =>
            node != null &&
            node.DescendantNodes()
                .Any(descendant => descendant.IsAnyKind(SyntaxKind.IfStatement,
                    SyntaxKind.ConditionalExpression,
                    SyntaxKind.CoalesceExpression,
                    SyntaxKind.SwitchStatement,
                    SyntaxKindEx.SwitchExpression,
                    SyntaxKindEx.CoalesceAssignmentExpression));

        public static object FindConstantValue(this SyntaxNode node, SemanticModel semanticModel) =>
            new CSharpConstantValueFinder(semanticModel).FindConstant(node);

        public static string FindStringConstant(this SyntaxNode node, SemanticModel semanticModel) =>
            FindConstantValue(node, semanticModel) as string;

        public static bool IsPartOfBinaryNegationOrCondition(this SyntaxNode node)
        {
            if (!(node.Parent is MemberAccessExpressionSyntax))
            {
                return false;
            }

            var topNode = node.Parent.GetSelfOrTopParenthesizedExpression();
            if (topNode.Parent?.IsKind(SyntaxKind.BitwiseNotExpression) ?? false)
            {
                return true;
            }

            var current = topNode;
            while (!current.Parent?.IsAnyKind(SyntaxKind.BitwiseNotExpression,
                                              SyntaxKind.IfStatement,
                                              SyntaxKind.WhileStatement,
                                              SyntaxKind.ConditionalExpression,
                                              SyntaxKind.MethodDeclaration,
                                              SyntaxKind.SimpleLambdaExpression) ?? false)
            {
                current = current.Parent;
            }

            return current.Parent switch
            {
                IfStatementSyntax ifStatement => ifStatement.Condition == current,
                WhileStatementSyntax whileStatement => whileStatement.Condition == current,
                ConditionalExpressionSyntax condExpr => condExpr.Condition == current,
                _ => false
            };
        }

        public static string GetDeclarationTypeName(this SyntaxNode node) =>
            node.Kind() switch
            {
                SyntaxKind.ClassDeclaration => "class",
                SyntaxKind.StructDeclaration => "struct",
                SyntaxKind.InterfaceDeclaration => "interface",
                SyntaxKindEx.RecordClassDeclaration => "record",
                SyntaxKindEx.RecordStructDeclaration => "record struct",
                _ => GetUnknownType(node.Kind())
            };

        // Extracts the expression body from an arrow-bodied syntax node.
        public static ArrowExpressionClauseSyntax ArrowExpressionBody(this SyntaxNode node) =>
            node switch
            {
                MethodDeclarationSyntax a => a.ExpressionBody,
                ConstructorDeclarationSyntax b => b.ExpressionBody(),
                OperatorDeclarationSyntax c => c.ExpressionBody,
                AccessorDeclarationSyntax d => d.ExpressionBody(),
                ConversionOperatorDeclarationSyntax e => e.ExpressionBody,
                _ => null
            };

        public static SyntaxNode RemoveParentheses(this SyntaxNode expression)
        {
            var current = expression;
            while (current is { } && current.IsAnyKind(SyntaxKind.ParenthesizedExpression, SyntaxKindEx.ParenthesizedPattern))
            {
                current = current.IsKind(SyntaxKindEx.ParenthesizedPattern)
                    ? ((ParenthesizedPatternSyntaxWrapper)current).Pattern
                    : ((ParenthesizedExpressionSyntax)current).Expression;
            }
            return current;
        }

        public static SyntaxNode WalkUpParentheses(this SyntaxNode node)
        {
            while (node is not null && node.IsKind(SyntaxKind.ParenthesizedExpression))
            {
                node = node.Parent;
            }
            return node;
        }

        /// <summary>
        /// Finds the syntactic complementing <see cref="SyntaxNode"/> of an assignment with tuples.
        /// <code>
        /// var (a, b) = (1, 2);      // if node is a, 1 is returned and vice versa.
        /// (var a, var b) = (1, 2);  // if node is 2, var b is returned and vice versa.
        /// </code>
        /// <paramref name="node"/> must be an <see cref="ArgumentSyntax"/> of a tuple or some variable designation of a <see cref="SyntaxKindEx.DeclarationExpression"/>.
        /// </summary>
        public static SyntaxNode FindAssignmentComplement(this SyntaxNode node)
        {
            Debug.Assert(node.IsAnyKind(SyntaxKind.Argument,
                                        SyntaxKindEx.DiscardDesignation,
                                        SyntaxKindEx.SingleVariableDesignation,
                                        SyntaxKindEx.ParenthesizedVariableDesignation), "Only direct children of tuple like elements are allowed.");
            Debug.Assert(node is not ArgumentSyntax || node.Parent.IsKind(SyntaxKindEx.TupleExpression), "Only arguments of a tuple are supported.");
            // can be either outermost tuple, or DeclarationExpression if 'node' is SingleVariableDesignationExpression
            var outermostParenthesesExpression = node.Ancestors()
                .TakeWhile(x => x.IsAnyKind(
                    SyntaxKind.Argument,
                    SyntaxKindEx.TupleExpression,
                    SyntaxKindEx.SingleVariableDesignation,
                    SyntaxKindEx.ParenthesizedVariableDesignation,
                    SyntaxKindEx.DiscardDesignation,
                    SyntaxKindEx.DeclarationExpression))
                .LastOrDefault();
            if ((TupleExpressionSyntaxWrapper.IsInstance(outermostParenthesesExpression) || DeclarationExpressionSyntaxWrapper.IsInstance(outermostParenthesesExpression))
                && outermostParenthesesExpression.Parent is AssignmentExpressionSyntax assignment)
            {
                var otherSide = assignment switch
                {
                    { Left: { } left, Right: { } right } when left.Equals(outermostParenthesesExpression) => right,
                    { Left: { } left, Right: { } right } when right.Equals(outermostParenthesesExpression) => left,
                    _ => null,
                };
                if (TupleExpressionSyntaxWrapper.IsInstance(otherSide) || DeclarationExpressionSyntaxWrapper.IsInstance(otherSide))
                {
                    var stackFromNodeToOutermost = GetNestingPathFromNodeToOutermost(node);
                    return FindMatchingNestedNode(stackFromNodeToOutermost, otherSide);
                }
            }

            return null;

            static Stack<IndexCountPair> GetNestingPathFromNodeToOutermost(SyntaxNode node)
            {
                Stack<IndexCountPair> pathFromNodeToTheTop = new();
                while (TupleExpressionSyntaxWrapper.IsInstance(node?.Parent) || ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(node?.Parent))
                {
                    node = node switch
                    {
                        ArgumentSyntax tupleArgument when TupleExpressionSyntaxWrapper.IsInstance(node.Parent) =>
                            PushIndexAndCountTuple(pathFromNodeToTheTop, (TupleExpressionSyntaxWrapper)node.Parent, tupleArgument),
                        _ when VariableDesignationSyntaxWrapper.IsInstance(node) && ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(node.Parent) =>
                            PushIndexAndCountParenthesizedDesignation(pathFromNodeToTheTop, (ParenthesizedVariableDesignationSyntaxWrapper)node.Parent, (VariableDesignationSyntaxWrapper)node),
                        _ => null,
                    };
                    if (DeclarationExpressionSyntaxWrapper.IsInstance(node?.Parent) && node is { Parent.Parent: ArgumentSyntax { } argument })
                    {
                        node = argument;
                    }
                }
                return pathFromNodeToTheTop;
            }

            static SyntaxNode FindMatchingNestedNode(Stack<IndexCountPair> pathFromOutermostToGivenNode, SyntaxNode outermostParenthesesToMatch)
            {
                var matchedNestedNode = outermostParenthesesToMatch;
                while (matchedNestedNode is not null && pathFromOutermostToGivenNode.Count > 0)
                {
                    if (DeclarationExpressionSyntaxWrapper.IsInstance(matchedNestedNode))
                    {
                        matchedNestedNode = ((DeclarationExpressionSyntaxWrapper)matchedNestedNode).Designation;
                    }
                    var expectedPathPosition = pathFromOutermostToGivenNode.Pop();
                    matchedNestedNode = matchedNestedNode switch
                    {
                        _ when TupleExpressionSyntaxWrapper.IsInstance(matchedNestedNode) => StepDownInTuple((TupleExpressionSyntaxWrapper)matchedNestedNode, expectedPathPosition),
                        _ when ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(matchedNestedNode) =>
                            StepDownInParenthesizedVariableDesignation((ParenthesizedVariableDesignationSyntaxWrapper)matchedNestedNode, expectedPathPosition),
                        _ => null,
                    };
                }
                return matchedNestedNode;
            }

            static SyntaxNode PushIndexAndCountTuple(Stack<IndexCountPair> indexAndCount, TupleExpressionSyntaxWrapper tuple, ArgumentSyntax argument)
            {
                indexAndCount.Push(new(tuple.Arguments.IndexOf(argument), tuple.Arguments.Count));
                return tuple.SyntaxNode.Parent;
            }

            static SyntaxNode PushIndexAndCountParenthesizedDesignation(Stack<IndexCountPair> indexAndCount,
                                                                        ParenthesizedVariableDesignationSyntaxWrapper parenthesizedDesignation,
                                                                        VariableDesignationSyntaxWrapper variable)
            {
                indexAndCount.Push(new(parenthesizedDesignation.Variables.IndexOf(variable), parenthesizedDesignation.Variables.Count));
                return parenthesizedDesignation.SyntaxNode;
            }

            static SyntaxNode StepDownInParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntaxWrapper parenthesizedVariableDesignation, IndexCountPair expectedPathPosition) =>
                parenthesizedVariableDesignation.Variables.Count == expectedPathPosition.TupleLength
                    ? (SyntaxNode)parenthesizedVariableDesignation.Variables[expectedPathPosition.Index]
                    : null;

            static SyntaxNode StepDownInTuple(TupleExpressionSyntaxWrapper tupleExpression, IndexCountPair expectedPathPosition) =>
                tupleExpression.Arguments.Count == expectedPathPosition.TupleLength
                    ? (SyntaxNode)tupleExpression.Arguments[expectedPathPosition.Index].Expression
                    : null;
        }

        private static string GetUnknownType(SyntaxKind kind) =>

#if DEBUG

            throw new System.ArgumentException($"Unexpected type {kind}", nameof(kind));

#else

            "type";

#endif

        private readonly record struct IndexCountPair(int Index, int TupleLength);

        private sealed class ControlFlowGraphCache : ControlFlowGraphCacheBase
        {
            protected override bool IsLocalFunction(SyntaxNode node) =>
                node.IsKind(SyntaxKindEx.LocalFunctionStatement);

            protected override bool HasNestedCfg(SyntaxNode node) =>
                node.IsAnyKind(SyntaxKindEx.LocalFunctionStatement, SyntaxKind.SimpleLambdaExpression, SyntaxKind.AnonymousMethodExpression, SyntaxKind.ParenthesizedLambdaExpression);
        }
    }
}
