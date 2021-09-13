﻿/*
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
            var ret = new RoslynState();
            ProcessBlockInternal(block, ret);
            return ret;
        }

        private void ProcessBlockInternal(BasicBlock block, RoslynState state)
        {
            foreach (var operation in block.OperationsAndBranchValue.ToReversedExecutionOrder())
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
                }
            }
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

        private static ISymbol ParameterOrLocalSymbol(IOperation operation) =>
            operation switch
            {
                var _ when IParameterReferenceOperationWrapper.IsInstance(operation) => IParameterReferenceOperationWrapper.FromOperation(operation).Parameter,
                var _ when ILocalReferenceOperationWrapper.IsInstance(operation) => ILocalReferenceOperationWrapper.FromOperation(operation).Local,
                _ => null
            };

        private class RoslynState : State
        {
//FIXME: Move Logic here
        }
    }
}
