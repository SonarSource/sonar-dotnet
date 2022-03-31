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

using System;
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
        private readonly Graph graph;

        protected override BasicBlock ExitBlock => Cfg.ExitBlock;

        public RoslynLiveVariableAnalysis(ControlFlowGraph cfg) : base(cfg, OriginalDeclaration(cfg.OriginalOperation))
        {
            graph = new Graph(cfg);
            Analyze();
        }

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

        protected override IEnumerable<BasicBlock> ReversedBlocks() =>
            Cfg.Blocks.Reverse();

        protected override IEnumerable<BasicBlock> Predecessors(BasicBlock block) =>
            graph[block].Predecessors;

        protected override IEnumerable<BasicBlock> Successors(BasicBlock block) =>
            graph[block].Successors;

        protected override State ProcessBlock(BasicBlock block)
        {
            var ret = new RoslynState(this);
            ret.ProcessBlock(Cfg, block);
            return ret;
        }

        public override bool IsLocal(ISymbol symbol) =>
            originalDeclaration.Equals(symbol?.ContainingSymbol);

        private IEnumerable<ControlFlowBranch> TryRegionSuccessors(ControlFlowRegion finallyRegion)
        {
            var tryRegion = finallyRegion.EnclosingRegion.NestedRegions.Single(x => x.Kind == ControlFlowRegionKind.Try);
            return tryRegion.Blocks(Cfg).SelectMany(x => x.Successors).Where(x => x.FinallyRegions.Contains(finallyRegion));
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

        private static ISymbol OriginalDeclaration(IOperation originalOperation)
        {
            if (originalOperation.IsAnyKind(OperationKindEx.MethodBody, OperationKindEx.Block, OperationKindEx.ConstructorBody))
            {
                var syntax = originalOperation.Syntax.IsKind(SyntaxKindEx.ArrowExpressionClause) ? originalOperation.Syntax.Parent : originalOperation.Syntax;
                return new IOperationWrapperSonar(originalOperation).SemanticModel.GetDeclaredSymbol(syntax);
            }
            else
            {
                return originalOperation switch
                {
                    var _ when IAnonymousFunctionOperationWrapper.IsInstance(originalOperation) => IAnonymousFunctionOperationWrapper.FromOperation(originalOperation).Symbol,
                    var _ when ILocalFunctionOperationWrapper.IsInstance(originalOperation) => ILocalFunctionOperationWrapper.FromOperation(originalOperation).Symbol,
                    _ => throw new NotSupportedException($"Operations of kind: {originalOperation.Kind} are not supported.")
                };
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
                    if (reference.IsOutArgument())
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
                    if (owner.ParameterOrLocalSymbol(operation) is { } symbol)
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

        private class Graph
        {
            private readonly ControlFlowGraph cfg;
            private readonly Dictionary<int, Node> nodes = new();

            public Node this[BasicBlock block] => nodes[block.Ordinal];

            public Graph(ControlFlowGraph cfg)
            {
                this.cfg = cfg;
                foreach (var block in cfg.Blocks)
                {
                    nodes.Add(block.Ordinal, new());
                }
                foreach (var block in cfg.Blocks)
                {
                    Process(block);
                }
            }

            private void Process(BasicBlock block)
            {
                foreach (var successor in block.Successors)
                {
                    if (successor.Destination != null)
                    {
                        ProcessDirectDestination(successor);
                    }
                    else if (successor.Source.EnclosingRegion is { Kind: ControlFlowRegionKind.Finally } finallyRegion)
                    {
                        ProcessFinallyRegion(successor.Source, finallyRegion);
                    }
                }
                if (block.IsEnclosedIn(ControlFlowRegionKind.Try))
                {
                    foreach (var catchOrFilterRegion in block.Successors.SelectMany(CatchOrFilterRegions))
                    {
                        AddBranch(block, cfg.Blocks[catchOrFilterRegion.FirstBlockOrdinal]);
                    }
                }
            }

            private void ProcessDirectDestination(ControlFlowBranch branch)
            {
                if (branch.FinallyRegions.Any())
                {   // When exiting finally region, redirect to finally instead of the normal destination
                    foreach (var finallyRegion in branch.FinallyRegions)    // FIXME: All of them or just the first one?
                    {
                        AddBranch(branch.Source, cfg.Blocks[finallyRegion.FirstBlockOrdinal]);
                    }
                }
                else
                {
                    AddBranch(branch.Source, branch.Destination);
                }
            }

            private void ProcessFinallyRegion(BasicBlock source, ControlFlowRegion finallyRegion)
            {
                // Redirect exit from throw and finally to following blocks.
                foreach (var trySuccessor in TryRegionSuccessors(source.EnclosingRegion))
                {
                    AddBranch(source, trySuccessor.Destination);
                }
            }

            private IEnumerable<ControlFlowBranch> TryRegionSuccessors(ControlFlowRegion finallyRegion) =>
                TryRegion(finallyRegion).Blocks(cfg).SelectMany(x => x.Successors).Where(x => x.FinallyRegions.Contains(finallyRegion));

            private void AddBranch(BasicBlock source, BasicBlock destination)
            {
                nodes[source.Ordinal].Successors.Add(destination);
                nodes[destination.Ordinal].Predecessors.Add(source);
            }
        }

        private class Node
        {
            public readonly List<BasicBlock> Predecessors = new();
            public readonly List<BasicBlock> Successors = new();
        }
    }
}
