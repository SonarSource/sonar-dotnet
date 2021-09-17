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

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.CFG.Roslyn
{
    public class BasicBlock
    {
        private static readonly ConditionalWeakTable<object, BasicBlock> InstanceCache = new ConditionalWeakTable<object, BasicBlock>();
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

        private readonly Lazy<IOperation> branchValue;
        private readonly Lazy<ControlFlowBranch> conditionalSuccessor;
        private readonly Lazy<ControlFlowConditionKind> conditionKind;
        private readonly Lazy<ControlFlowRegion> enclosingRegion;
        private readonly Lazy<ControlFlowBranch> fallThroughSuccessor;
        private readonly Lazy<bool> isReachable;
        private readonly Lazy<BasicBlockKind> kind;
        private readonly Lazy<ImmutableArray<IOperation>> operations;
        private readonly Lazy<int> ordinal;
        private readonly Lazy<ImmutableArray<ControlFlowBranch>> predecessors;
        private readonly Lazy<ImmutableArray<ControlFlowBranch>> successors;
        private readonly Lazy<ImmutableArray<BasicBlock>> successorBlocks;
        private readonly Lazy<ImmutableArray<IOperation>> operationsAndBranchValue;

        public IOperation BranchValue => branchValue.Value;
        public ControlFlowBranch ConditionalSuccessor => conditionalSuccessor.Value;
        public ControlFlowConditionKind ConditionKind => conditionKind.Value;
        public ControlFlowRegion EnclosingRegion => enclosingRegion.Value;
        public ControlFlowBranch FallThroughSuccessor => fallThroughSuccessor.Value;
        public bool IsReachable => isReachable.Value;
        public BasicBlockKind Kind => kind.Value;
        public ImmutableArray<IOperation> Operations => operations.Value;
        public int Ordinal => ordinal.Value;
        public ImmutableArray<ControlFlowBranch> Predecessors => predecessors.Value;
        public ImmutableArray<ControlFlowBranch> Successors => successors.Value;
        public ImmutableArray<BasicBlock> SuccessorBlocks => successorBlocks.Value;
        public ImmutableArray<IOperation> OperationsAndBranchValue => operationsAndBranchValue.Value;

        static BasicBlock()
        {
            if (RoslynHelper.FlowAnalysisType("BasicBlock") is { } type)
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
            _ = instance ?? throw new ArgumentNullException(nameof(instance));
            branchValue = BranchValueProperty.ReadValue<IOperation>(instance);
            conditionalSuccessor = ConditionalSuccessorProperty.ReadValue(instance, ControlFlowBranch.Wrap);
            conditionKind = ConditionKindProperty.ReadValue<ControlFlowConditionKind>(instance);
            enclosingRegion = EnclosingRegionProperty.ReadValue(instance, ControlFlowRegion.Wrap);
            fallThroughSuccessor = FallThroughSuccessorProperty.ReadValue(instance, ControlFlowBranch.Wrap);
            isReachable = IsReachableProperty.ReadValue<bool>(instance);
            kind = KindProperty.ReadValue<BasicBlockKind>(instance);
            operations = OperationsProperty.ReadImmutableArray<IOperation>(instance);
            ordinal = OrdinalProperty.ReadValue<int>(instance);
            predecessors = PredecessorsProperty.ReadImmutableArray(instance, ControlFlowBranch.Wrap);
            successors = new Lazy<ImmutableArray<ControlFlowBranch>>(() =>
            {
                // since Roslyn does not differentiate between pattern types in CFG, it builds unreachable block for missing
                // pattern match even when discard pattern option is presented. In this case we explicitly exclude this branch
                if (SwitchExpressionArmSyntaxWrapper.IsInstance(BranchValue?.Syntax) && DiscardPatternSyntaxWrapper.IsInstance(((SwitchExpressionArmSyntaxWrapper)BranchValue.Syntax).Pattern))
                {
                    return FallThroughSuccessor != null ? ImmutableArray.Create(FallThroughSuccessor) : ImmutableArray<ControlFlowBranch>.Empty;
                }
                else
                {
                    return ImmutableArray.CreateRange(new[] { FallThroughSuccessor, ConditionalSuccessor }.Where(x => x != null));
                }
            });
            successorBlocks = new Lazy<ImmutableArray<BasicBlock>>(() => Successors.Select(x => x.Destination).Where(x => x != null).ToImmutableArray());
            operationsAndBranchValue = new Lazy<ImmutableArray<IOperation>>(() =>
            {
                if (BranchValue == null)
                {
                    return Operations;
                }
                var builder = Operations.ToBuilder();
                builder.Add(BranchValue);
                return builder.ToImmutable();
            });
        }

        public static BasicBlock Wrap(object instance) =>
            instance == null ? null : InstanceCache.GetValue(instance, x => new BasicBlock(x));
    }
}
