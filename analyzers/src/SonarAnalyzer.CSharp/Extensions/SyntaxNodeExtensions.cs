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

using System;
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

        public static ControlFlowGraph CreateCfg(this SyntaxNode body, SemanticModel semanticModel)
        {
            var operation = semanticModel.GetOperation(body.Parent);
            var rootSyntax = operation.RootOperation().Syntax;
            var cfg = ControlFlowGraph.Create(rootSyntax, semanticModel);
            if (body.Parent.IsAnyKind(SyntaxKindEx.LocalFunctionStatement, SyntaxKind.SimpleLambdaExpression, SyntaxKind.AnonymousMethodExpression, SyntaxKind.ParenthesizedLambdaExpression))
            {
                // We need to go up and track all possible enclosing lambdas, local functions and other FlowAnonymousFunctionOperations
                var cfgFlowOperations = cfg.FlowAnonymousFunctionOperations();  // Avoid recomputing for ancestors that do not produce FlowAnonymousFunction
                foreach (var node in body.Parent.AncestorsAndSelf().TakeWhile(x => x != rootSyntax).Reverse())
                {
                    if (node.IsKind(SyntaxKindEx.LocalFunctionStatement))
                    {
                        cfg = cfg.GetLocalFunctionControlFlowGraph(node);
                        cfgFlowOperations = cfg.FlowAnonymousFunctionOperations();
                    }
                    else if (cfgFlowOperations.SingleOrDefault(x => x.WrappedOperation.Syntax == node) is var flowOperation && flowOperation.WrappedOperation != null)
                    {
                        cfg = cfg.GetAnonymousFunctionControlFlowGraph(flowOperation);
                        cfgFlowOperations = cfg.FlowAnonymousFunctionOperations();
                    }
                    else if (node == body)  // 'body' should always reach  LocalFunction CFG or AnonymousFunction CFG above
                    {
                        throw new InvalidOperationException($"Could not find CFG for {body.Parent.Kind()} in {body.SyntaxTree.FilePath}:{body.GetLocation().GetLineNumberToReport()}");
                    }
                }
            }
            return cfg;
        }

        private static string GetUnknownType(SyntaxKind kind)
        {
#if DEBUG
            throw new ArgumentException($"Unexpected type {kind}", nameof(kind));
#else
            return "type";
#endif
        }
    }
}
