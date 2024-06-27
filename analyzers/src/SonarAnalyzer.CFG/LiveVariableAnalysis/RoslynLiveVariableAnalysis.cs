/*
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

using Microsoft.CodeAnalysis.Semantics;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.CFG.LiveVariableAnalysis;

public sealed class RoslynLiveVariableAnalysis : LiveVariableAnalysisBase<ControlFlowGraph, BasicBlock>
{
    private readonly Dictionary<CaptureId, List<IOperation>> flowCaptureOperations = [];
    private readonly Dictionary<int, List<BasicBlock>> blockPredecessors = [];
    private readonly Dictionary<int, List<BasicBlock>> blockSuccessors = [];

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

    public ISymbol ParameterOrLocalSymbol(IOperation operation)
    {
        ISymbol candidate = operation switch
        {
            _ when IParameterReferenceOperationWrapper.IsInstance(operation) => IParameterReferenceOperationWrapper.FromOperation(operation).Parameter,
            _ when ILocalReferenceOperationWrapper.IsInstance(operation) => ILocalReferenceOperationWrapper.FromOperation(operation).Local,
            _ => null
        };
        return IsLocal(candidate) ? candidate : null;
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
        ret.ProcessBlock(Cfg, block, flowCaptureOperations);
        return ret;
    }

    private void ResolveCaptures()
    {
        foreach (var operation in Cfg.Blocks
                                    .Where(x => x.EnclosingRegion.EnclosingRegionOrSelf(ControlFlowRegionKind.LocalLifetime) is not null)
                                    .SelectMany(x => x.OperationsAndBranchValue)
                                    .ToExecutionOrder())
        {
            if (IFlowCaptureOperationWrapper.IsInstance(operation.Instance))
            {
                var flowCapture = IFlowCaptureOperationWrapper.FromOperation(operation.Instance);

                if (IFlowCaptureReferenceOperationWrapper.IsInstance(flowCapture.Value)
                    && IFlowCaptureReferenceOperationWrapper.FromOperation(flowCapture.Value) is var captureReference
                    && flowCaptureOperations.TryGetValue(captureReference.Id, out var capturedOperations))
                {
                    AppendFlowCaptureReference(flowCapture.Id, capturedOperations.ToArray());
                }
                else
                {
                    AppendFlowCaptureReference(flowCapture.Id, flowCapture.Value);
                }
            }
        }

        void AppendFlowCaptureReference(CaptureId id, params IOperation[] operations)
        {
            if (!flowCaptureOperations.TryGetValue(id, out var list))
            {
                list = [];
                flowCaptureOperations.Add(id, list);
            }
            list.AddRange(operations);
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
                var _ when IAnonymousFunctionOperationWrapper.IsInstance(originalOperation) => IAnonymousFunctionOperationWrapper.FromOperation(originalOperation).Symbol,
                var _ when ILocalFunctionOperationWrapper.IsInstance(originalOperation) => ILocalFunctionOperationWrapper.FromOperation(originalOperation).Symbol,
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

        public void ProcessBlock(ControlFlowGraph cfg, BasicBlock block, Dictionary<CaptureId, List<IOperation>> flowCaptureOperations)
        {
            foreach (var operation in block.OperationsAndBranchValue.ToReversedExecutionOrder().Select(x => x.Instance))
            {
                if ((IFlowCaptureOperationWrapper.IsInstance(operation)
                        && flowCaptureOperations.TryGetValue(IFlowCaptureOperationWrapper.FromOperation(operation).Id, out var captured))
                    || (IFlowCaptureReferenceOperationWrapper.IsInstance(operation)
                        && flowCaptureOperations.TryGetValue(IFlowCaptureReferenceOperationWrapper.FromOperation(operation).Id, out captured)))
                {
                    foreach (var capturedOperation in captured)
                    {
                        ProcessOperation(cfg, capturedOperation);
                    }
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
                ProcessCaptured(cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunction, owner.Cancel));
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
