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
            var thisSide = node.Ancestors()
                .TakeWhile(x => x.IsAnyKind(
                    SyntaxKind.Argument,
                    SyntaxKindEx.TupleExpression,
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
                    node = node switch
                    {
                        _ when TupleExpressionSyntaxWrapper.IsInstance(node.Parent) => PushTupleElementIndexAndCount(indexAndCount, node),
                        _ when ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(node.Parent) => PushParenthesizedVariableDesignationIndexAndCount(indexAndCount, node),
                        _ => null,
                    };
                    if (DeclarationExpressionSyntaxWrapper.IsInstance(node?.Parent) && node is { Parent.Parent: ArgumentSyntax { } argument })
                    {
                        node = argument;
                    }
                }
                return indexAndCount;
            }

            static SyntaxNode FindMatchingNestedNode(Stack<IndexCountPair> indexAndCount, SyntaxNode node)
            {
                while (node is not null && indexAndCount.Count > 0)
                {
                    if (DeclarationExpressionSyntaxWrapper.IsInstance(node))
                    {
                        node = ((DeclarationExpressionSyntaxWrapper)node).Designation;
                    }
                    var indexCountPair = indexAndCount.Pop();
                    node = node switch
                    {
                        _ when TupleExpressionSyntaxWrapper.IsInstance(node) => StepDownInTuple((TupleExpressionSyntaxWrapper)node, indexCountPair),
                        _ when ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(node) =>
                            StepDownInParenthesizedVariableDesignation((ParenthesizedVariableDesignationSyntaxWrapper)node, indexCountPair),
                        _ => null,
                    };
                }
                return node;
            }

            static SyntaxNode StepDownInParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntaxWrapper parenthesizedVariableDesignation, IndexCountPair indexCountPair) =>
                parenthesizedVariableDesignation.Variables.Count == indexCountPair.Count
                    ? (SyntaxNode)parenthesizedVariableDesignation.Variables[indexCountPair.Index]
                    : null;

            static SyntaxNode StepDownInTuple(TupleExpressionSyntaxWrapper tupleExpression, IndexCountPair indexCountPair) =>
                tupleExpression.Arguments.Count == indexCountPair.Count
                    ? tupleExpression.Arguments[indexCountPair.Index].Expression
                    : (SyntaxNode)null;

            static SyntaxNode PushTupleElementIndexAndCount(Stack<IndexCountPair> indexAndCount, SyntaxNode node)
            {
                var parentTuple = (TupleExpressionSyntaxWrapper)node.Parent;
                indexAndCount.Push(new(parentTuple.Arguments.IndexOf((ArgumentSyntax)node), parentTuple.Arguments.Count));
                return parentTuple.SyntaxNode.Parent;
            }

            static SyntaxNode PushParenthesizedVariableDesignationIndexAndCount(Stack<IndexCountPair> indexAndCount, SyntaxNode node)
            {
                var parentDesignation = (ParenthesizedVariableDesignationSyntaxWrapper)node.Parent;
                indexAndCount.Push(new(parentDesignation.Variables.IndexOf((VariableDesignationSyntaxWrapper)node), parentDesignation.Variables.Count));
                return parentDesignation.SyntaxNode;
            }
        }

        private static string GetUnknownType(SyntaxKind kind) =>

#if DEBUG

            throw new System.ArgumentException($"Unexpected type {kind}", nameof(kind));

#else

            "type";

#endif

        private readonly record struct IndexCountPair(int Index, int Count);

        private sealed class ControlFlowGraphCache : ControlFlowGraphCacheBase
        {
            protected override bool IsLocalFunction(SyntaxNode node) =>
                node.IsKind(SyntaxKindEx.LocalFunctionStatement);

            protected override bool HasNestedCfg(SyntaxNode node) =>
                node.IsAnyKind(SyntaxKindEx.LocalFunctionStatement, SyntaxKind.SimpleLambdaExpression, SyntaxKind.AnonymousMethodExpression, SyntaxKind.ParenthesizedLambdaExpression);
        }
    }
}
