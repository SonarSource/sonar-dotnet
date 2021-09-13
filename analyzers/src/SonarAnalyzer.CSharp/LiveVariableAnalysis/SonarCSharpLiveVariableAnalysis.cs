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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.LiveVariableAnalysis.CSharp
{
    public sealed class SonarCSharpLiveVariableAnalysis : LiveVariableAnalysisBase<IControlFlowGraph, Block>
    {
        private readonly ISymbol declaration;
        private readonly SemanticModel semanticModel;

        protected override Block ExitBlock => cfg.ExitBlock;

        public SonarCSharpLiveVariableAnalysis(IControlFlowGraph controlFlowGraph, ISymbol declaration, SemanticModel semanticModel) : base(controlFlowGraph)
        {
            this.declaration = declaration;
            this.semanticModel = semanticModel;
            Analyze();
        }

        internal static bool IsOutArgument(IdentifierNameSyntax identifier) =>
            identifier.GetFirstNonParenthesizedParent() is ArgumentSyntax argument && argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword);

        internal static bool IsLocalScoped(ISymbol symbol, ISymbol declaration)
        {
            return IsLocalOrParameterSymbol()
                && symbol.ContainingSymbol != null
                && symbol.ContainingSymbol.Equals(declaration);

            bool IsLocalOrParameterSymbol() =>
                (symbol is ILocalSymbol local && local.RefKind() == RefKind.None)
                || (symbol is IParameterSymbol parameter && parameter.RefKind == RefKind.None);
        }

        protected override IEnumerable<Block> ReversedBlocks() =>
            cfg.Blocks.Reverse();

        protected override IEnumerable<Block> Successors(Block block) =>
            block.SuccessorBlocks;

        protected override IEnumerable<Block> Predecessors(Block block) =>
            block.PredecessorBlocks;

        protected override State ProcessBlock(Block block)
        {
            var ret = new SonarState(declaration, semanticModel);
            ret.ProcessBlock(block);
            return ret;
        }

        private class SonarState : State
        {
            private readonly ISymbol declaration;
            private readonly SemanticModel semanticModel;

            public ISet<SyntaxNode> AssignmentLhs { get; } = new HashSet<SyntaxNode>();

            public SonarState(ISymbol declaration, SemanticModel semanticModel)
            {
                this.declaration = declaration;
                this.semanticModel = semanticModel;
            }

            public void ProcessBlock(Block block)
            {
                foreach (var instruction in block.Instructions.Reverse())
                {
                    switch (instruction.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                            ProcessIdentifier((IdentifierNameSyntax)instruction);
                            break;

                        case SyntaxKind.GenericName:
                            ProcessGenericName((GenericNameSyntax)instruction);
                            break;

                        case SyntaxKind.SimpleAssignmentExpression:
                            ProcessSimpleAssignment((AssignmentExpressionSyntax)instruction);
                            break;

                        case SyntaxKind.VariableDeclarator:
                            ProcessVariableDeclarator((VariableDeclaratorSyntax)instruction);
                            break;

                        case SyntaxKind.AnonymousMethodExpression:
                        case SyntaxKind.ParenthesizedLambdaExpression:
                        case SyntaxKind.SimpleLambdaExpression:
                        case SyntaxKind.QueryExpression:
                            CollectAllCapturedLocal(instruction);
                            break;
                    }
                }

                if (block.Instructions.Any())
                {
                    return;
                }

                // Variable declaration in a foreach statement is not a VariableDeclarator, so handling it separately:
                if (block is BinaryBranchBlock foreachBlock && foreachBlock.BranchingNode.IsKind(SyntaxKind.ForEachStatement))
                {
                    var foreachNode = (ForEachStatementSyntax)foreachBlock.BranchingNode;
                    ProcessVariableInForeach(foreachNode);
                }

                // Keep alive the variables declared and used in the using statement until the UsingFinalizerBlock
                if (block is UsingEndBlock usingFinalizerBlock)
                {
                    var disposableSymbols = usingFinalizerBlock.Identifiers
                        .Select(x => semanticModel.GetDeclaredSymbol(x.Parent) ?? semanticModel.GetSymbolInfo(x.Parent).Symbol)
                        .WhereNotNull();
                    foreach (var disposableSymbol in disposableSymbols)
                    {
                        UsedBeforeAssigned.Add(disposableSymbol);
                    }
                }
            }

            private void ProcessVariableInForeach(ForEachStatementSyntax foreachNode)
            {
                if (semanticModel.GetDeclaredSymbol(foreachNode) is { } symbol)
                {
                    Assigned.Add(symbol);
                    UsedBeforeAssigned.Remove(symbol);
                }
            }

            private void ProcessVariableDeclarator(VariableDeclaratorSyntax instruction)
            {
                if (semanticModel.GetDeclaredSymbol(instruction) is { } symbol)
                {
                    Assigned.Add(symbol);
                    UsedBeforeAssigned.Remove(symbol);
                }
            }

            private void ProcessSimpleAssignment(AssignmentExpressionSyntax assignment)
            {
                var left = assignment.Left.RemoveParentheses();
                if (left.IsKind(SyntaxKind.IdentifierName)
                    && semanticModel.GetSymbolInfo(left).Symbol is { } symbol
                    && IsLocal(symbol))
                {
                    AssignmentLhs.Add(left);
                    Assigned.Add(symbol);
                    UsedBeforeAssigned.Remove(symbol);
                }
            }

            private void ProcessIdentifier(IdentifierNameSyntax identifier)
            {
                if (!identifier.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(semanticModel)
                    && semanticModel.GetSymbolInfo(identifier).Symbol is { } symbol)
                {
                    if (IsLocal(symbol))
                    {
                        if (IsOutArgument(identifier))
                        {
                            Assigned.Add(symbol);
                            UsedBeforeAssigned.Remove(symbol);
                        }
                        else if (!AssignmentLhs.Contains(identifier))
                        {
                            UsedBeforeAssigned.Add(symbol);
                        }
                    }

                    if (symbol is IMethodSymbol { MethodKind: MethodKindEx.LocalFunction } method)
                    {
                        ProcessLocalFunction(symbol);
                    }
                }
            }

            private void ProcessGenericName(GenericNameSyntax genericName)
            {
                if (!genericName.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(semanticModel)
                    && semanticModel.GetSymbolInfo(genericName).Symbol is IMethodSymbol { MethodKind: MethodKindEx.LocalFunction } method)
                {
                    ProcessLocalFunction(method);
                }
            }

            private void ProcessLocalFunction(ISymbol symbol)
            {
                if (!ProcessedLocalFunctions.Contains(symbol)
                    && symbol.DeclaringSyntaxReferences.Length == 1
                    && symbol.DeclaringSyntaxReferences.Single().GetSyntax() is { } node
                    && (LocalFunctionStatementSyntaxWrapper)node is LocalFunctionStatementSyntaxWrapper function
                    && CSharpControlFlowGraph.TryGet(function.Body ?? function.ExpressionBody as CSharpSyntaxNode, semanticModel, out var cfg))
                {
                    ProcessedLocalFunctions.Add(symbol);
                    foreach (var block in cfg.Blocks.Reverse())
                    {
                        ProcessBlock(block);
                    }
                }
            }

            private void CollectAllCapturedLocal(SyntaxNode instruction)
            {
                var allCapturedSymbols = instruction.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Select(i => semanticModel.GetSymbolInfo(i).Symbol)
                    .Where(s => s != null && IsLocal(s));

                // Collect captured locals
                // Read and write both affects liveness
                Captured.UnionWith(allCapturedSymbols);
            }

            private bool IsLocal(ISymbol symbol) =>
                IsLocalScoped(symbol, declaration);
        }
    }
}
