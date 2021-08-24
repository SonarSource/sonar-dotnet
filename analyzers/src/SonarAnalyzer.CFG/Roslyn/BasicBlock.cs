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
using System.Reflection;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.CFG.Roslyn
{
    public class BasicBlock
    {
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

        public BasicBlock(object instance)
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
        }
    }
}
