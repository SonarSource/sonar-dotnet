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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Extensions
{
    internal static partial class SyntaxNodeExtensions
    {
        public static bool IsPartOfBinaryNegationOrCondition(this SyntaxNode node)
        {
            if (!(node.Parent is MemberAccessExpressionSyntax))
            {
                return false;
            }

            var current = node;
            while (current.Parent != null && !current.Parent.IsAnyKind(SyntaxKind.IfStatement, SyntaxKind.WhileStatement))
            {
                current = current.Parent;
            }

            return current.Parent switch
            {
                IfStatementSyntax ifStatement => ifStatement.Condition == current,
                WhileStatementSyntax whileStatement => whileStatement.Condition == current,
                _ => false
            };
        }

        public static object FindConstantValue(this SyntaxNode node, SemanticModel semanticModel) =>
            new VisualBasicConstantValueFinder(semanticModel).FindConstant(node);

        public static string FindStringConstant(this SyntaxNode node, SemanticModel semanticModel) =>
            FindConstantValue(node, semanticModel) as string;

        public static ControlFlowGraph CreateCfg(this SyntaxNode block, SemanticModel semanticModel)
        {
            var operation = semanticModel.GetOperation(block);
            var rootSyntax = operation.RootOperation().Syntax;
            var cfg = ControlFlowGraph.Create(rootSyntax, semanticModel);
            if (block is LambdaExpressionSyntax)
            {
                // We need to go up and track all possible enclosing lambdas and other FlowAnonymousFunctionOperations
                var cfgFlowOperations = cfg.FlowAnonymousFunctionOperations();  // Avoid recomputing for ancestors that do not produce FlowAnonymousFunction
                foreach (var node in block.AncestorsAndSelf().TakeWhile(x => x != rootSyntax).Reverse())
                {
                    if (cfgFlowOperations.SingleOrDefault(x => x.WrappedOperation.Syntax == node) is var flowOperation && flowOperation.WrappedOperation != null)
                    {
                        cfg = cfg.GetAnonymousFunctionControlFlowGraph(flowOperation);
                        cfgFlowOperations = cfg.FlowAnonymousFunctionOperations();
                    }
                    else if (node == block)
                    {
                        return null;    // Lambda syntax is not always recognized as FlowOperation for invalid syntaxes
                    }
                }
            }
            return cfg;
        }
    }
}
