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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.LiveVariableAnalysis.CSharp
{
    public sealed class CSharpLiveVariableAnalysis : AbstractLiveVariableAnalysis
    {
        private readonly ISymbol declaration;
        private readonly SemanticModel semanticModel;

        private CSharpLiveVariableAnalysis(IControlFlowGraph controlFlowGraph, ISymbol declaration, SemanticModel semanticModel) : base(controlFlowGraph)
        {
            this.declaration = declaration;
            this.semanticModel = semanticModel;
        }

        public static AbstractLiveVariableAnalysis Analyze(IControlFlowGraph controlFlowGraph, ISymbol declaration, SemanticModel semanticModel)
        {
            var lva = new CSharpLiveVariableAnalysis(controlFlowGraph, declaration, semanticModel);
            lva.PerformAnalysis();
            return lva;
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

        protected override State ProcessBlock(Block block)
        {
            var ret = new State();
            ProcessBlockInternal(block, ret);
            return ret;
        }

        private void ProcessBlockInternal(Block block, State state)
        {
            foreach (var instruction in block.Instructions.Reverse())
            {
                switch (instruction.Kind())
                {
                    case SyntaxKind.IdentifierName:
                        ProcessIdentifier((IdentifierNameSyntax)instruction, state);
                        break;

                    case SyntaxKind.GenericName:
                        ProcessGenericName(instruction, state);
                        break;

                    case SyntaxKind.SimpleAssignmentExpression:
                        ProcessSimpleAssignment((AssignmentExpressionSyntax)instruction, state);
                        break;

                    case SyntaxKind.VariableDeclarator:
                        ProcessVariableDeclarator((VariableDeclaratorSyntax)instruction, state);
                        break;

                    case SyntaxKind.AnonymousMethodExpression:
                    case SyntaxKind.ParenthesizedLambdaExpression:
                    case SyntaxKind.SimpleLambdaExpression:
                    case SyntaxKind.QueryExpression:
                        CollectAllCapturedLocal(instruction, state);
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
                ProcessVariableInForeach(foreachNode, state);
            }

            // Keep alive the variables declared and used in the using statement until the UsingFinalizerBlock
            if (block is UsingEndBlock usingFinalizerBlock)
            {
                var disposableSymbols = usingFinalizerBlock.Identifiers
                    .Select(i => semanticModel.GetDeclaredSymbol(i.Parent)
                                ?? semanticModel.GetSymbolInfo(i.Parent).Symbol)
                    .WhereNotNull();
                foreach (var disposableSymbol in disposableSymbols)
                {
                    state.UsedBeforeAssigned.Add(disposableSymbol);
                }
            }
        }

        private void ProcessVariableInForeach(ForEachStatementSyntax foreachNode, State state)
        {
            if (semanticModel.GetDeclaredSymbol(foreachNode) is { } symbol)
            {
                state.Assigned.Add(symbol);
                state.UsedBeforeAssigned.Remove(symbol);
            }
        }

        private void ProcessVariableDeclarator(VariableDeclaratorSyntax instruction, State state)
        {
            if (semanticModel.GetDeclaredSymbol(instruction) is { } symbol)
            {
                state.Assigned.Add(symbol);
                state.UsedBeforeAssigned.Remove(symbol);
            }
        }

        private void ProcessSimpleAssignment(AssignmentExpressionSyntax assignment, State state)
        {
            var left = assignment.Left.RemoveParentheses();
            if (left.IsKind(SyntaxKind.IdentifierName)
                && semanticModel.GetSymbolInfo(left).Symbol is { } symbol
                && IsLocalScoped(symbol))
            {
                state.AssignmentLhs.Add(left);
                state.Assigned.Add(symbol);
                state.UsedBeforeAssigned.Remove(symbol);
            }
        }

        private void ProcessIdentifier(IdentifierNameSyntax identifier, State state)
        {
            if (!identifier.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(semanticModel)
                && semanticModel.GetSymbolInfo(identifier).Symbol is { } symbol)
            {
                if (IsLocalScoped(symbol))
                {
                    if (IsOutArgument(identifier))
                    {
                        state.Assigned.Add(symbol);
                        state.UsedBeforeAssigned.Remove(symbol);
                    }
                    else if (!state.AssignmentLhs.Contains(identifier))
                    {
                        state.UsedBeforeAssigned.Add(symbol);
                    }
                }

                if (symbol is IMethodSymbol method && method.MethodKind == MethodKindEx.LocalFunction)
                {
                    ProcessLocalFunction(symbol, state);
                }
            }
        }

        private void ProcessGenericName(SyntaxNode genericName, State state)
        {
            if (genericName.Parent.IsKind(SyntaxKind.InvocationExpression)
                && semanticModel.GetSymbolInfo(genericName).Symbol is IMethodSymbol method
                && method.MethodKind == MethodKindEx.LocalFunction)
            {
                ProcessLocalFunction(method, state);
            }
        }

        private void ProcessLocalFunction(ISymbol symbol, State state)
        {
            if (!state.ProcessedLocalFunctions.Contains(symbol)
                && symbol.DeclaringSyntaxReferences.Length == 1
                && symbol.DeclaringSyntaxReferences.Single().GetSyntax() is { } node
                && (LocalFunctionStatementSyntaxWrapper)node is LocalFunctionStatementSyntaxWrapper function
                && CSharpControlFlowGraph.TryGet(function.Body ?? function.ExpressionBody as CSharpSyntaxNode, semanticModel, out var cfg))
            {
                state.ProcessedLocalFunctions.Add(symbol);
                foreach (var block in cfg.Blocks.Reverse())
                {
                    ProcessBlockInternal(block, state);
                }
            }
        }

        private void CollectAllCapturedLocal(SyntaxNode instruction, State state)
        {
            var allCapturedSymbols = instruction.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(i => semanticModel.GetSymbolInfo(i).Symbol)
                .Where(s => s != null && IsLocalScoped(s));

            // Collect captured locals
            // Read and write both affects liveness
            state.CapturedVariables.UnionWith(allCapturedSymbols);
        }

        private bool IsLocalScoped(ISymbol symbol) =>
            IsLocalScoped(symbol, declaration);
    }
}
