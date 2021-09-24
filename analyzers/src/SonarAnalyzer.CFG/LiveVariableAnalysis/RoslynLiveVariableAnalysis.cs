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
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.CFG.LiveVariableAnalysis
{
    public sealed class RoslynLiveVariableAnalysis : LiveVariableAnalysisBase<ControlFlowGraph, BasicBlock>
    {
        protected override BasicBlock ExitBlock => cfg.ExitBlock;

        public RoslynLiveVariableAnalysis(ControlFlowGraph cfg)
            : base(cfg, new IOperationWrapperSonar(cfg.OriginalOperation).SemanticModel.GetDeclaredSymbol(cfg.OriginalOperation.Syntax)) =>
            Analyze();

        public ISymbol ParameterOrLocalSymbol(IOperation operation)
        {
            ISymbol candidate = operation switch
            {
                var _ when IParameterReferenceOperationWrapper.IsInstance(operation) => IParameterReferenceOperationWrapper.FromOperation(operation).Parameter,
                var _ when ILocalReferenceOperationWrapper.IsInstance(operation) => ILocalReferenceOperationWrapper.FromOperation(operation).Local,
                _ => null
            };
            return IsLocal(candidate) ? candidate : null;
        }

        public static bool IsOutArgument(IOperation operation) =>
            new IOperationWrapperSonar(operation) is var wrapped
            && IArgumentOperationWrapper.IsInstance(wrapped.Parent)
            && IArgumentOperationWrapper.FromOperation(wrapped.Parent).Parameter.RefKind == RefKind.Out;

        protected override IEnumerable<BasicBlock> ReversedBlocks() =>
            cfg.Blocks.Reverse();

        protected override IEnumerable<BasicBlock> Successors(BasicBlock block)
        {
            foreach (var successor in block.Successors)
            {
                if (successor.Destination != null && successor.FinallyRegions.Any())
                {   // When exiting finally region, redirect to finally instead of the normal destination
                    foreach (var finallyRegion in successor.FinallyRegions)
                    {
                        yield return cfg.Blocks[finallyRegion.FirstBlockOrdinal];
                    }
                }
                else if (successor.Destination != null)
                {
                    yield return successor.Destination;
                }
                else if (successor.Source.EnclosingRegion.Kind == ControlFlowRegionKind.Finally)
                {   // Redirect exit from throw and finally to following blocks.
                    foreach (var trySuccessor in TryRegionSuccessors(block.EnclosingRegion))
                    {
                        yield return trySuccessor.Destination;
                    }
                }
            }
            if (block.EnclosingRegion.Kind == ControlFlowRegionKind.Try)
            {
                foreach (var catchOrFilterRegion in block.Successors.SelectMany(CatchOrFilterRegions))
                {
                    yield return cfg.Blocks[catchOrFilterRegion.FirstBlockOrdinal];
                }
            }
        }

        protected override IEnumerable<BasicBlock> Predecessors(BasicBlock block)
        {
            if (block.Predecessors.Any())
            {
                foreach (var predecessor in block.Predecessors)
                {
                    // When exiting finally region, redirect predecessor to the source of StructuredEceptionHandling branches
                    if (predecessor.FinallyRegions.Any())
                    {
                        foreach (var structuredExceptionHandling in StructuredExceptionHandlinBranches(predecessor.FinallyRegions))
                        {
                            yield return structuredExceptionHandling.Source;
                        }
                    }
                    else
                    {
                        yield return predecessor.Source;
                    }
                }
            }
            else if (block.EnclosingRegion.Kind == ControlFlowRegionKind.Finally && block.Ordinal == block.EnclosingRegion.FirstBlockOrdinal)
            {
                // Link first block of FinallyRegion to the source of all branches exiting that FinallyRegion
                foreach (var trySuccessor in TryRegionSuccessors(block.EnclosingRegion))
                {
                    yield return trySuccessor.Source;
                }
            }

            IEnumerable<ControlFlowBranch> StructuredExceptionHandlinBranches(IEnumerable<ControlFlowRegion> finallyRegions) =>
                finallyRegions.Select(x => cfg.Blocks[x.LastBlockOrdinal].Successors.SingleOrDefault(x => x.Semantics == ControlFlowBranchSemantics.StructuredExceptionHandling))
                    .Where(x => x != null);
        }

        protected override State ProcessBlock(BasicBlock block)
        {
            var ret = new RoslynState(this);
            ret.ProcessBlock(cfg, block);
            return ret;
        }

        public override bool IsLocal(ISymbol symbol) =>
            originalDeclaration.Equals(symbol?.ContainingSymbol);

        private IEnumerable<ControlFlowBranch> TryRegionSuccessors(ControlFlowRegion finallyRegion)
        {
            var tryRegion = finallyRegion.EnclosingRegion.NestedRegions.Single(x => x.Kind == ControlFlowRegionKind.Try);
            return tryRegion.Blocks(cfg).SelectMany(x => x.Successors).Where(x => x.FinallyRegions.Contains(finallyRegion));
        }

        private static IEnumerable<ControlFlowRegion> CatchOrFilterRegions(ControlFlowBranch trySuccessor) =>
            trySuccessor.Semantics == ControlFlowBranchSemantics.Throw
                ? CatchOrFilterRegions(trySuccessor.Source.EnclosingRegion.EnclosingRegion)
                : trySuccessor.LeavingRegions.Where(x => x.Kind == ControlFlowRegionKind.TryAndCatch).SelectMany(CatchOrFilterRegions);

        private static IEnumerable<ControlFlowRegion> CatchOrFilterRegions(ControlFlowRegion tryAndCatchRegion)
        {
            foreach (var region in tryAndCatchRegion.NestedRegions)
            {
                if (region.Kind == ControlFlowRegionKind.Catch)
                {
                    yield return region;
                }
                else if (region.Kind == ControlFlowRegionKind.FilterAndHandler)
                {
                    yield return region.NestedRegions.Single(x => x.Kind == ControlFlowRegionKind.Filter);
                }
            }
        }

        private class RoslynState : State
        {
            private readonly RoslynLiveVariableAnalysis owner;
            private readonly ISet<ISymbol> capturedLocalFunctions = new HashSet<ISymbol>();

            public RoslynState(RoslynLiveVariableAnalysis owner) =>
                this.owner = owner;

            public void ProcessBlock(ControlFlowGraph cfg, BasicBlock block)
            {
                foreach (var operation in block.OperationsAndBranchValue.ToReversedExecutionOrder())
                {
                    // Everything that is added to this switch needs to be considered inside ProcessCaptured as well
                    switch (operation.Instance.Kind)
                    {
                        case OperationKindEx.LocalReference:
                            ProcessParameterOrLocalReference(ILocalReferenceOperationWrapper.FromOperation(operation.Instance));
                            break;
                        case OperationKindEx.ParameterReference:
                            ProcessParameterOrLocalReference(IParameterReferenceOperationWrapper.FromOperation(operation.Instance));
                            break;
                        case OperationKindEx.SimpleAssignment:
                            ProcessSimpleAssignment(ISimpleAssignmentOperationWrapper.FromOperation(operation.Instance));
                            break;
                        case OperationKindEx.FlowAnonymousFunction:
                            ProcessFlowAnonymousFunction(cfg, IFlowAnonymousFunctionOperationWrapper.FromOperation(operation.Instance));
                            break;
                        case OperationKindEx.Invocation:
                            ProcessLocalFunction(cfg, IInvocationOperationWrapper.FromOperation(operation.Instance).TargetMethod);
                            break;
                        case OperationKindEx.MethodReference:
                            ProcessLocalFunction(cfg, IMethodReferenceOperationWrapper.FromOperation(operation.Instance).Method);
                            break;
                    }
                }
            }

            private void ProcessParameterOrLocalReference(IOperationWrapper reference)
            {
                if (owner.ParameterOrLocalSymbol(reference.WrappedOperation) is { } symbol)
                {
                    if (IsOutArgument(reference.WrappedOperation))
                    {
                        Assigned.Add(symbol);
                        UsedBeforeAssigned.Remove(symbol);
                    }
                    else if (!reference.IsAssignmentTarget())
                    {
                        UsedBeforeAssigned.Add(symbol);
                    }
                }
            }

            private void ProcessSimpleAssignment(ISimpleAssignmentOperationWrapper assignment)
            {
                if (owner.ParameterOrLocalSymbol(assignment.Target) is { } localTarget)
                {
                    Assigned.Add(localTarget);
                    UsedBeforeAssigned.Remove(localTarget);
                }
            }

            private void ProcessFlowAnonymousFunction(ControlFlowGraph cfg, IFlowAnonymousFunctionOperationWrapper anonymousFunction)
            {
                if (!anonymousFunction.Symbol.IsStatic) // Performance: No need to descent into static
                {
                    ProcessCaptured(cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunction));
                }
            }

            private void ProcessCaptured(ControlFlowGraph cfg)
            {
                foreach (var operation in cfg.Blocks.SelectMany(x => x.OperationsAndBranchValue).SelectMany(x => x.DescendantsAndSelf()))
                {
                    if (ParameterOrLocalSymbol(operation) is { } symbol)
                    {
                        Captured.Add(symbol);
                    }
                    else
                    {
                        switch (operation.Kind)
                        {
                            case OperationKindEx.FlowAnonymousFunction:
                                ProcessFlowAnonymousFunction(cfg, IFlowAnonymousFunctionOperationWrapper.FromOperation(operation));
                                break;
                            case OperationKindEx.Invocation:
                                ProcessCapturedLocalFunction(cfg, IInvocationOperationWrapper.FromOperation(operation).TargetMethod);
                                break;
                            case OperationKindEx.MethodReference:
                                ProcessCapturedLocalFunction(cfg, IMethodReferenceOperationWrapper.FromOperation(operation).Method);
                                break;
                        }
                    }
                }
            }

            private void ProcessCapturedLocalFunction(ControlFlowGraph cfg, IMethodSymbol method)
            {
                if (HandleLocalFunction(capturedLocalFunctions, method) is { } localFunction)
                {
                    capturedLocalFunctions.Add(localFunction);
                    ProcessCaptured(cfg.FindLocalFunctionCfgInScope(localFunction));
                }
            }

            private void ProcessLocalFunction(ControlFlowGraph cfg, IMethodSymbol method)
            {
                if (HandleLocalFunction(ProcessedLocalFunctions, method) is { } localFunction)
                {
                    ProcessedLocalFunctions.Add(localFunction);
                    var localFunctionCfg = cfg.FindLocalFunctionCfgInScope(localFunction);
                    foreach (var block in localFunctionCfg.Blocks.Reverse())    // Simplified approach, ignoring branching and try/catch/finally flows
                    {
                        ProcessBlock(localFunctionCfg, block);
                    }
                }
            }

            private static IMethodSymbol HandleLocalFunction(ISet<ISymbol> processed, IMethodSymbol method)
            {
                // We need ConstructedFrom because TargetMethod of a generic local function invocation is not the correct symbol (has IsDefinition=False and wrong ContainingSymbol)
                if (method.ConstructedFrom is { MethodKind: MethodKindEx.LocalFunction, IsStatic: false } localFunction
                    && !processed.Contains(localFunction))
                {
                    processed.Add(localFunction);
                    return localFunction;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
