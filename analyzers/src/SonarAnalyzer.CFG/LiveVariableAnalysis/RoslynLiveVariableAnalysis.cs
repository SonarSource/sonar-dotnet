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
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.CFG.LiveVariableAnalysis
{
    public sealed class RoslynLiveVariableAnalysis : LiveVariableAnalysisBase<ControlFlowGraph, BasicBlock>
    {
        protected override BasicBlock ExitBlock => cfg.ExitBlock;

        public RoslynLiveVariableAnalysis(ControlFlowGraph cfg) : base(cfg) =>
            Analyze();

        protected override IEnumerable<BasicBlock> ReversedBlocks() =>
            cfg.Blocks.Reverse();

        protected override IEnumerable<BasicBlock> Successors(BasicBlock block) =>
            block.SuccessorBlocks;

        protected override IEnumerable<BasicBlock> Predecessors(BasicBlock block) =>
            block.Predecessors.Select(x => x.Source);

        internal static bool IsOutArgument(IOperation operation) =>
            new IOperationWrapperSonar(operation) is var wrapped
            && IArgumentOperationWrapper.IsInstance(wrapped.Parent)
            && IArgumentOperationWrapper.FromOperation(wrapped.Parent).Parameter.RefKind == RefKind.Out;

        protected override State ProcessBlock(BasicBlock block)
        {
            //FIXME: Ugly
            var ret = new State();
            ProcessBlockInternal(block, ret);
            return ret;
        }

        private void ProcessBlockInternal(BasicBlock block, State state)
        {
            foreach (var operation in block.OperationsAndBranchValue.Reverse().ToExecutionOrder())
            {
                switch (operation.Instance.Kind)
                {
                    case OperationKindEx.LocalReference:
                        ProcessParameterOrLocalReference(state, ILocalReferenceOperationWrapper.FromOperation(operation.Instance));
                        break;
                    case OperationKindEx.ParameterReference:
                        ProcessParameterOrLocalReference(state, IParameterReferenceOperationWrapper.FromOperation(operation.Instance));
                        break;
                    case OperationKindEx.SimpleAssignment:
                        ProcessSimpleAssignment(state, ISimpleAssignmentOperationWrapper.FromOperation(operation.Instance));
                        break;
                    case OperationKindEx.FlowAnonymousFunction:
                        ProcessFlowAnonymousFunction(state, cfg, IFlowAnonymousFunctionOperationWrapper.FromOperation(operation.Instance));
                        break;

                        //FIXME: Something is still missing around here

                        //            case SyntaxKind.GenericName:
                        //                ProcessGenericName((GenericNameSyntax)instruction, state);
                        //                break;

                }
            }
            //FIXME: Something is still missing around here

            //    // Keep alive the variables declared and used in the using statement until the UsingFinalizerBlock
            //    if (block is UsingEndBlock usingFinalizerBlock)
            //    {
            //        var disposableSymbols = usingFinalizerBlock.Identifiers
            //            .Select(i => semanticModel.GetDeclaredSymbol(i.Parent)
            //                        ?? semanticModel.GetSymbolInfo(i.Parent).Symbol)
            //            .WhereNotNull();
            //        foreach (var disposableSymbol in disposableSymbols)
            //        {
            //            state.UsedBeforeAssigned.Add(disposableSymbol);
            //        }
            //    }
        }

        private void ProcessParameterOrLocalReference(State state, IOperationWrapper reference)
        {
            var symbol = ParameterOrLocalSymbol(reference.WrappedOperation);
            Debug.Assert(symbol != null, "Only supported types should be passed to ParameterOrLocalSymbol");
            if (IsOutArgument(reference.WrappedOperation))
            {
                state.Assigned.Add(symbol);
                state.UsedBeforeAssigned.Remove(symbol);
            }
            else if (!IsAssignmentTarget())
            {
                state.UsedBeforeAssigned.Add(symbol);
            }

            bool IsAssignmentTarget() =>
                new IOperationWrapperSonar(reference.WrappedOperation).Parent is { } parent
                && parent.Kind == OperationKindEx.SimpleAssignment
                && ISimpleAssignmentOperationWrapper.FromOperation(parent).Target == reference.WrappedOperation;
        }

        private void ProcessSimpleAssignment(State state, ISimpleAssignmentOperationWrapper assignment)
        {
            if (ParameterOrLocalSymbol(assignment.Target) is { } localTarget)
            {
                state.Assigned.Add(localTarget);
                state.UsedBeforeAssigned.Remove(localTarget);
            }
        }

        private void ProcessFlowAnonymousFunction(State state, ControlFlowGraph cfg, IFlowAnonymousFunctionOperationWrapper anonymousFunction)
        {
            var anonymousFunctionCfg = cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunction);
            foreach (var operation in anonymousFunctionCfg.Blocks.SelectMany(x => x.OperationsAndBranchValue).SelectMany(x => x.DescendantsAndSelf()))
            {
                switch (operation.Kind)
                {
                    case OperationKindEx.LocalReference:
                        state.Captured.Add(ILocalReferenceOperationWrapper.FromOperation(operation).Local);
                        break;
                    case OperationKindEx.ParameterReference:
                        state.Captured.Add(IParameterReferenceOperationWrapper.FromOperation(operation).Parameter);
                        break;
                    case OperationKindEx.FlowAnonymousFunction:
                        ProcessFlowAnonymousFunction(state, anonymousFunctionCfg, IFlowAnonymousFunctionOperationWrapper.FromOperation(operation));
                        break;
                }
            }
        }

        //private void ProcessIdentifier(IdentifierNameSyntax identifier, State state)
        //{
        //    if (!identifier.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(semanticModel)
        //        && semanticModel.GetSymbolInfo(identifier).Symbol is { } symbol)
        //    {
        //        if (IsLocalScoped(symbol))
        //        {
        //            if (IsOutArgument(identifier))
        //            {
        //                state.Assigned.Add(symbol);
        //                state.UsedBeforeAssigned.Remove(symbol);
        //            }
        //            else if (!state.AssignmentLhs.Contains(identifier))
        //            {
        //                state.UsedBeforeAssigned.Add(symbol);
        //            }
        //        }

        //        if (symbol is IMethodSymbol { MethodKind: MethodKindEx.LocalFunction } method)
        //        {
        //            ProcessLocalFunction(symbol, state);
        //        }
        //    }
        //}

        //private void ProcessGenericName(GenericNameSyntax genericName, State state)
        //{
        //    if (!genericName.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(semanticModel)
        //        && semanticModel.GetSymbolInfo(genericName).Symbol is IMethodSymbol {MethodKind: MethodKindEx.LocalFunction } method)
        //    {
        //        ProcessLocalFunction(method, state);
        //    }
        //}

        //private void ProcessLocalFunction(ISymbol symbol, State state)
        //{
        //    if (!state.ProcessedLocalFunctions.Contains(symbol)
        //        && symbol.DeclaringSyntaxReferences.Length == 1
        //        && symbol.DeclaringSyntaxReferences.Single().GetSyntax() is { } node
        //        && (LocalFunctionStatementSyntaxWrapper)node is LocalFunctionStatementSyntaxWrapper function
        //        && CSharpControlFlowGraph.TryGet(function.Body ?? function.ExpressionBody as CSharpSyntaxNode, semanticModel, out var cfg))
        //    {
        //        state.ProcessedLocalFunctions.Add(symbol);
        //        foreach (var block in cfg.Blocks.Reverse())
        //        {
        //            ProcessBlockInternal(block, state);
        //        }
        //    }
        //}

        private static ISymbol ParameterOrLocalSymbol(IOperation operation) =>
            operation switch
            {
                var _ when IParameterReferenceOperationWrapper.IsInstance(operation) => IParameterReferenceOperationWrapper.FromOperation(operation).Parameter,
                var _ when ILocalReferenceOperationWrapper.IsInstance(operation) => ILocalReferenceOperationWrapper.FromOperation(operation).Local,
                _ => null
            };
    }
}
