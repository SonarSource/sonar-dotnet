/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Reflection;
using System.Runtime.CompilerServices;

namespace SonarAnalyzer.CFG.Roslyn
{
    public class BasicBlock
    {
        private static readonly ConditionalWeakTable<object, BasicBlock> InstanceCache = new();
        private static readonly PropertyInfo BranchValueProperty;
        private static readonly PropertyInfo ConditionalSuccessorProperty;
        private static readonly PropertyInfo ConditionKindProperty;
        private static readonly PropertyInfo EnclosingRegionProperty;
        private static readonly PropertyInfo FallThroughSuccessorProperty;
        private static readonly PropertyInfo IsReachableProperty;
        private static readonly PropertyInfo KindProperty;
        private static readonly PropertyInfo OperationsProperty;
        private static readonly PropertyInfo OrdinalProperty;
        private static readonly PropertyInfo PredecessorsProperty;

        private readonly object instance;
        private readonly Lazy<ImmutableArray<ControlFlowBranch>> successors;
        private readonly Lazy<ImmutableArray<BasicBlock>> successorBlocks;
        private readonly Lazy<ImmutableArray<IOperation>> operationsAndBranchValue;
        private IOperation branchValue;
        private ControlFlowBranch conditionalSuccessor;
        private ControlFlowConditionKind? conditionKind;
        private ControlFlowRegion enclosingRegion;
        private ControlFlowBranch fallThroughSuccessor;
        private bool? isReachable;
        private BasicBlockKind? kind;
        private ImmutableArray<IOperation> operations;
        private int? ordinal;
        private ImmutableArray<ControlFlowBranch> predecessors;

        public IOperation BranchValue => BranchValueProperty.ReadCached(instance, ref branchValue);
        public ControlFlowBranch ConditionalSuccessor => ConditionalSuccessorProperty.ReadCached(instance, ControlFlowBranch.Wrap, ref conditionalSuccessor);
        public ControlFlowConditionKind ConditionKind => ConditionKindProperty.ReadCached(instance, ref conditionKind);
        public ControlFlowRegion EnclosingRegion => EnclosingRegionProperty.ReadCached(instance, ControlFlowRegion.Wrap, ref enclosingRegion);
        public ControlFlowBranch FallThroughSuccessor => FallThroughSuccessorProperty.ReadCached(instance, ControlFlowBranch.Wrap, ref fallThroughSuccessor);
        public bool IsReachable => IsReachableProperty.ReadCached(instance, ref isReachable);
        public BasicBlockKind Kind => KindProperty.ReadCached(instance, ref kind);
        public ImmutableArray<IOperation> Operations => OperationsProperty.ReadCached(instance, ref operations);
        public int Ordinal => OrdinalProperty.ReadCached(instance, ref ordinal);
        public ImmutableArray<ControlFlowBranch> Predecessors => PredecessorsProperty.ReadCached(instance, ControlFlowBranch.Wrap, ref predecessors);
        public ImmutableArray<ControlFlowBranch> Successors => successors.Value;
        public ImmutableArray<BasicBlock> SuccessorBlocks => successorBlocks.Value;
        public ImmutableArray<IOperation> OperationsAndBranchValue => operationsAndBranchValue.Value;

        static BasicBlock()
        {
            if (TypeLoader.FlowAnalysisType("BasicBlock") is { } type)
            {
                BranchValueProperty = type.GetProperty(nameof(BranchValue));
                ConditionalSuccessorProperty = type.GetProperty(nameof(ConditionalSuccessor));
                ConditionKindProperty = type.GetProperty(nameof(ConditionKind));
                EnclosingRegionProperty = type.GetProperty(nameof(EnclosingRegion));
                FallThroughSuccessorProperty = type.GetProperty(nameof(FallThroughSuccessor));
                IsReachableProperty = type.GetProperty(nameof(IsReachable));
                KindProperty = type.GetProperty(nameof(Kind));
                OperationsProperty = type.GetProperty(nameof(Operations));
                OrdinalProperty = type.GetProperty(nameof(Ordinal));
                PredecessorsProperty = type.GetProperty(nameof(Predecessors));
            }
        }

        private BasicBlock(object instance)
        {
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
            successors = new Lazy<ImmutableArray<ControlFlowBranch>>(() =>
            {
                // since Roslyn does not differentiate between pattern types in CFG, it builds unreachable block for missing
                // pattern match even when discard pattern option is presented. In this case we explicitly exclude this branch
                if (SwitchExpressionArmSyntaxWrapper.IsInstance(BranchValue?.Syntax) && DiscardPatternSyntaxWrapper.IsInstance(((SwitchExpressionArmSyntaxWrapper)BranchValue.Syntax).Pattern))
                {
                    return FallThroughSuccessor is null ? ImmutableArray<ControlFlowBranch>.Empty : ImmutableArray.Create(FallThroughSuccessor);
                }
                else
                {
                    return new[] { FallThroughSuccessor, ConditionalSuccessor }.Where(x => x != null).ToImmutableArray();
                }
            });
            successorBlocks = new Lazy<ImmutableArray<BasicBlock>>(() => Successors.Select(x => x.Destination).Where(x => x != null).ToImmutableArray());
            operationsAndBranchValue = new Lazy<ImmutableArray<IOperation>>(() => BranchValue is null ? Operations : Operations.Add(BranchValue));
        }

        public static BasicBlock Wrap(object instance) =>
            instance == null ? null : InstanceCache.GetValue(instance, x => new BasicBlock(x));
    }
}
