﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.CFG.LiveVariableAnalysis;

public sealed class RoslynLiveVariableAnalysis : LiveVariableAnalysisBase<ControlFlowGraph, BasicBlock>
{
    private readonly Dictionary<CaptureId, List<ISymbol>> flowCaptures = [];
    private readonly Dictionary<int, List<BasicBlock>> blockPredecessors = [];
    private readonly Dictionary<int, List<BasicBlock>> blockSuccessors = [];

    internal ImmutableDictionary<int, List<BasicBlock>> BlockPredecessors => blockPredecessors.ToImmutableDictionary();

    protected override BasicBlock ExitBlock => Cfg.ExitBlock;

    public RoslynLiveVariableAnalysis(ControlFlowGraph cfg, CancellationToken cancel)
        : base(cfg, OriginalDeclaration(cfg.OriginalOperation), cancel)
    {
        foreach (var ordinal in cfg.Blocks.Select(x => x.Ordinal))
        {
            blockPredecessors.Add(ordinal, []);
            blockSuccessors.Add(ordinal, []);
        }
        foreach (var block in cfg.Blocks)
        {
            BuildBranches(block);
        }
        ResolveCaptures();
        Analyze();
    }

    public IEnumerable<ISymbol> ParameterOrLocalSymbols(IOperation operation)
    {
        var candidates = operation switch
        {
            _ when operation.AsParameterReference() is { } parameterReference => [parameterReference.Parameter],
            _ when operation.AsLocalReference() is { } localReference => [localReference.Local],
            _ when operation.AsFlowCaptureReference() is { } flowCaptureReference => flowCaptures.TryGetValue(flowCaptureReference.Id, out var symbols) ? symbols : [],
            _ => []
        };
        return candidates.Where(x => IsLocal(x));
    }

    public override bool IsLocal(ISymbol symbol) =>
        originalDeclaration.Equals(symbol?.ContainingSymbol);

    protected override IEnumerable<BasicBlock> ReversedBlocks() =>
        Cfg.Blocks.Reverse();

    protected override IEnumerable<BasicBlock> Predecessors(BasicBlock block) =>
        blockPredecessors[block.Ordinal];

    protected override IEnumerable<BasicBlock> Successors(BasicBlock block) =>
        blockSuccessors[block.Ordinal];

    protected override State ProcessBlock(BasicBlock block)
    {
        var ret = new RoslynState(this);
        ret.ProcessBlock(Cfg, block, flowCaptures);
        return ret;
    }

    private void ResolveCaptures()
    {
        foreach (var flowCapture in Cfg.Blocks
                                    .Where(x => x.EnclosingRegion.EnclosingRegionOrSelf(ControlFlowRegionKind.LocalLifetime) is not null)
                                    .SelectMany(x => x.OperationsAndBranchValue)
                                    .ToExecutionOrder()
                                    .Where(x => x.Instance.Kind == OperationKindEx.FlowCapture)
                                    .Select(x => x.Instance.ToFlowCapture()))
        {
            if (flowCapture.Value.AsFlowCaptureReference() is { } captureReference
                && flowCaptures.TryGetValue(captureReference.Id, out var symbols))
            {
                AppendFlowCaptureReference(flowCapture.Id, symbols);
            }
            else
            {
                AppendFlowCaptureReference(flowCapture.Id, ParameterOrLocalSymbols(flowCapture.Value));
            }
        }

        void AppendFlowCaptureReference(CaptureId id, IEnumerable<ISymbol> symbols)
        {
            if (!flowCaptures.TryGetValue(id, out var list))
            {
                list = [];
                flowCaptures.Add(id, list);
            }
            list.AddRange(symbols);
        }
    }

    private void BuildBranches(BasicBlock block)
    {
        foreach (var successor in block.Successors)
        {
            if (successor.Destination is not null)
            {
                // When exiting finally region, redirect to finally instead of the normal destination
                AddBranch(successor.Source, successor.FinallyRegions.Any() ? Cfg.Blocks[successor.FinallyRegions.First().FirstBlockOrdinal] : successor.Destination);
            }
            else if (successor.Source.EnclosingRegion is { Kind: ControlFlowRegionKind.Finally } finallyRegion)
            {
                BuildBranchesFinally(successor.Source, finallyRegion);
            }
        }
        if (block.EnclosingNonLocalLifetimeRegion() is { Kind: ControlFlowRegionKind.Try } tryRegion)
        {
            var catchesAll = false;
            foreach (var catchOrFilterRegion in block.Successors.SelectMany(CatchOrFilterRegions))
            {
                var catchOrFilterBlock = Cfg.Blocks[catchOrFilterRegion.FirstBlockOrdinal];
                AddBranch(block, catchOrFilterBlock);
                AddPredecessorsOutsideRegion(catchOrFilterBlock);
                catchesAll = catchesAll || (catchOrFilterRegion.Kind == ControlFlowRegionKind.Catch && IsCatchAllType(catchOrFilterRegion.ExceptionType));
            }
            if (!catchesAll && block.EnclosingRegion(ControlFlowRegionKind.TryAndFinally)?.NestedRegion(ControlFlowRegionKind.Finally) is { } finallyRegion)
            {
                var finallyBlock = Cfg.Blocks[finallyRegion.FirstBlockOrdinal];
                AddBranch(block, finallyBlock);
                AddPredecessorsOutsideRegion(finallyBlock);
            }
        }
        if (block.IsEnclosedIn(ControlFlowRegionKind.Catch) && block.Successors.Any(x => x.Semantics is ControlFlowBranchSemantics.Rethrow or ControlFlowBranchSemantics.Throw))
        {
            BuildBranchesRethrow(block);
        }

        void AddPredecessorsOutsideRegion(BasicBlock destination)
        {
            // We assume that current block can throw in its first operation. Therefore predecessors outside this tryRegion need to be redirected to catch/filter/finally
            foreach (var predecessor in block.Predecessors.Where(x => x.Source.Ordinal < tryRegion.FirstBlockOrdinal || x.Source.Ordinal > tryRegion.LastBlockOrdinal))
            {
                AddBranch(predecessor.Source, destination);
            }
        }
    }

    private void BuildBranchesFinally(BasicBlock source, ControlFlowRegion finallyRegion)
    {
        foreach (var trySuccessor in TryRegionSuccessors(source.EnclosingRegion))
        {
            // Redirect exit from finally to the next block
            var destination = trySuccessor.FinallyRegions.SkipWhile(x => x != finallyRegion).Skip(1).FirstOrDefault() is { } nextOuterFinally
                ? Cfg.Blocks[nextOuterFinally.FirstBlockOrdinal]    // Outer finally that directly follows this finally
                : trySuccessor.Destination;                         // Normal block directly after this finally
            AddBranch(source, destination);
        }
    }

    private void BuildBranchesRethrow(BasicBlock block)
    {
        var currentTryCatchRegion = block.EnclosingRegion(ControlFlowRegionKind.TryAndCatch);
        var reachableHandlerRegions = currentTryCatchRegion.NestedRegion(ControlFlowRegionKind.Try).ReachableHandlers();
        var reachableCatchAndFinallyBlocks = reachableHandlerRegions.Where(x => x.FirstBlockOrdinal > currentTryCatchRegion.LastBlockOrdinal).SelectMany(x => x.Blocks(Cfg));
        foreach (var catchBlock in reachableCatchAndFinallyBlocks.Where(x => x.EnclosingRegion.Kind is ControlFlowRegionKind.Catch))
        {
            AddBranch(block, catchBlock);
        }
        if (reachableCatchAndFinallyBlocks.FirstOrDefault() is { EnclosingRegion.Kind: ControlFlowRegionKind.Finally } finallyBlock)
        {
            AddBranch(block, finallyBlock);
        }
    }

    private void AddBranch(BasicBlock source, BasicBlock destination)
    {
        blockSuccessors[source.Ordinal].Add(destination);
        blockPredecessors[destination.Ordinal].Add(source);
    }

    private IEnumerable<ControlFlowBranch> TryRegionSuccessors(ControlFlowRegion finallyRegion)
    {
        var tryRegion = finallyRegion.EnclosingRegion.NestedRegion(ControlFlowRegionKind.Try);
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

    private static bool IsCatchAllType(ITypeSymbol exceptionType) =>
        exceptionType.SpecialType == SpecialType.System_Object  // catch { ... }
        || exceptionType is { ContainingNamespace: { Name: nameof(System), ContainingNamespace.IsGlobalNamespace: true }, Name: nameof(Exception) };  // catch(Exception) { ... }

    private static ISymbol OriginalDeclaration(IOperation originalOperation)
    {
        if (originalOperation.IsAnyKind(OperationKindEx.MethodBody, OperationKindEx.Block, OperationKindEx.ConstructorBody))
        {
            var syntax = originalOperation.Syntax.IsKind(SyntaxKindEx.ArrowExpressionClause) ? originalOperation.Syntax.Parent : originalOperation.Syntax;
            return originalOperation.ToSonar().SemanticModel.GetDeclaredSymbol(syntax);
        }
        else
        {
            return originalOperation switch
            {
                var _ when originalOperation.AsAnonymousFunction() is { } anonymousFunction => anonymousFunction.Symbol,
                var _ when originalOperation.AsLocalFunction() is { } localFunction => localFunction.Symbol,
                _ => throw new NotSupportedException($"Operations of kind: {originalOperation.Kind} are not supported.")
            };
        }
    }

    private sealed class RoslynState : State
    {
        private readonly RoslynLiveVariableAnalysis owner;
        private readonly ISet<ISymbol> capturedLocalFunctions = new HashSet<ISymbol>();

        public RoslynState(RoslynLiveVariableAnalysis owner) =>
            this.owner = owner;

        public void ProcessBlock(ControlFlowGraph cfg, BasicBlock block, Dictionary<CaptureId, List<ISymbol>> flowCaptureOperations)
        {
            foreach (var operation in block.OperationsAndBranchValue.ToReversedExecutionOrder().Select(x => x.Instance))
            {
                if (operation.AsFlowCaptureReference() is { } flowCaptureReference  && flowCaptureOperations.TryGetValue(flowCaptureReference.Id, out var symbols))
                {
                    UsedBeforeAssigned.UnionWith(symbols);
                }
                else
                {
                    ProcessOperation(cfg, operation);
                }
            }
        }

        private void ProcessOperation(ControlFlowGraph cfg, IOperation operation)
        {
            // Everything that is added to this switch needs to be considered inside ProcessCaptured as well
            switch (operation.Kind)
            {
                case OperationKindEx.LocalReference:
                    ProcessParameterOrLocalReference(ILocalReferenceOperationWrapper.FromOperation(operation));
                    break;
                case OperationKindEx.ParameterReference:
                    ProcessParameterOrLocalReference(IParameterReferenceOperationWrapper.FromOperation(operation));
                    break;
                case OperationKindEx.SimpleAssignment:
                    ProcessSimpleAssignment(ISimpleAssignmentOperationWrapper.FromOperation(operation));
                    break;
                case OperationKindEx.FlowAnonymousFunction:
                    ProcessFlowAnonymousFunction(cfg, IFlowAnonymousFunctionOperationWrapper.FromOperation(operation));
                    break;
                case OperationKindEx.Invocation:
                    ProcessLocalFunction(cfg, IInvocationOperationWrapper.FromOperation(operation).TargetMethod);
                    break;
                case OperationKindEx.MethodReference:
                    ProcessLocalFunction(cfg, IMethodReferenceOperationWrapper.FromOperation(operation).Method);
                    break;
            }
        }

        private void ProcessParameterOrLocalReference(IOperationWrapper reference)
        {
            var symbols = owner.ParameterOrLocalSymbols(reference.WrappedOperation);
            if (reference.IsOutArgument())
            {
                Assigned.UnionWith(symbols);
                UsedBeforeAssigned.ExceptWith(symbols);
            }
            else if (!reference.IsAssignmentTarget())
            {
                UsedBeforeAssigned.UnionWith(symbols);
            }
        }

        private void ProcessSimpleAssignment(ISimpleAssignmentOperationWrapper assignment)
        {
            var targets = owner.ParameterOrLocalSymbols(assignment.Target);
            Assigned.UnionWith(targets);
            UsedBeforeAssigned.ExceptWith(targets);
        }

        private void ProcessFlowAnonymousFunction(ControlFlowGraph cfg, IFlowAnonymousFunctionOperationWrapper anonymousFunction)
        {
            if (!anonymousFunction.Symbol.IsStatic) // Performance: No need to descent into static
            {
                ProcessCaptured(cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunction, owner.Cancel));
            }
        }

        private void ProcessCaptured(ControlFlowGraph cfg)
        {
            foreach (var operation in cfg.Blocks.SelectMany(x => x.OperationsAndBranchValue).SelectMany(x => x.DescendantsAndSelf()))
            {
                var symbols = owner.ParameterOrLocalSymbols(operation);
                if (symbols.Any())
                {
                    Captured.UnionWith(symbols);
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
                ProcessCaptured(cfg.FindLocalFunctionCfgInScope(localFunction, owner.Cancel));
            }
        }

        private void ProcessLocalFunction(ControlFlowGraph cfg, IMethodSymbol method)
        {
            if (HandleLocalFunction(ProcessedLocalFunctions, method) is { } localFunction)
            {
                ProcessedLocalFunctions.Add(localFunction);
                var localFunctionCfg = cfg.FindLocalFunctionCfgInScope(localFunction, owner.Cancel);
                foreach (var block in localFunctionCfg.Blocks.Reverse())    // Simplified approach, ignoring branching and try/catch/finally flows
                {
                    ProcessBlock(localFunctionCfg, block, []);
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
