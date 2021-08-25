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
using System.Runtime.CompilerServices;
using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.CFG.Roslyn
{
    public class ControlFlowBranch
    {
        private static readonly ConditionalWeakTable<object, ControlFlowBranch> InstanceCache = new ConditionalWeakTable<object, ControlFlowBranch>();
        private static readonly PropertyInfo SourceProperty;
        private static readonly PropertyInfo DestinationProperty;
        private static readonly PropertyInfo SemanticsProperty;
        private static readonly PropertyInfo IsConditionalSuccessorProperty;
        private static readonly PropertyInfo EnteringRegionsProperty;
        private static readonly PropertyInfo LeavingRegionsProperty;
        private static readonly PropertyInfo FinallyRegionsProperty;

        private readonly Lazy<BasicBlock> source;
        private readonly Lazy<BasicBlock> destination;
        private readonly Lazy<ControlFlowBranchSemantics> semantics;
        private readonly Lazy<bool> isConditionalSuccessor;
        private readonly Lazy<ImmutableArray<ControlFlowRegion>> enteringRegions;
        private readonly Lazy<ImmutableArray<ControlFlowRegion>> leavingRegions;
        private readonly Lazy<ImmutableArray<ControlFlowRegion>> finallyRegions;

        public BasicBlock Source => source.Value;
        public BasicBlock Destination => destination.Value;
        public ControlFlowBranchSemantics Semantics => semantics.Value;
        public bool IsConditionalSuccessor => isConditionalSuccessor.Value;
        public ImmutableArray<ControlFlowRegion> EnteringRegions => enteringRegions.Value;
        public ImmutableArray<ControlFlowRegion> LeavingRegions => leavingRegions.Value;
        public ImmutableArray<ControlFlowRegion> FinallyRegions => finallyRegions.Value;

        static ControlFlowBranch()
        {
            if (RoslynHelper.FlowAnalysisType("ControlFlowBranch") is { } type)
            {
                SourceProperty = type.GetProperty(nameof(Source));
                DestinationProperty = type.GetProperty(nameof(Destination));
                SemanticsProperty = type.GetProperty(nameof(Semantics));
                IsConditionalSuccessorProperty = type.GetProperty(nameof(IsConditionalSuccessor));
                EnteringRegionsProperty = type.GetProperty(nameof(EnteringRegions));
                LeavingRegionsProperty = type.GetProperty(nameof(LeavingRegions));
                FinallyRegionsProperty = type.GetProperty(nameof(FinallyRegions));
            }
        }

        private ControlFlowBranch(object instance)
        {
            _ = instance ?? throw new ArgumentNullException(nameof(instance));
            source = SourceProperty.ReadValue(instance, BasicBlock.Wrap);
            destination = DestinationProperty.ReadValue(instance, BasicBlock.Wrap);
            semantics = SemanticsProperty.ReadValue<ControlFlowBranchSemantics>(instance);
            isConditionalSuccessor = IsConditionalSuccessorProperty.ReadValue<bool>(instance);
            enteringRegions = EnteringRegionsProperty.ReadImmutableArray(instance, ControlFlowRegion.Wrap);
            leavingRegions = LeavingRegionsProperty.ReadImmutableArray(instance, ControlFlowRegion.Wrap);
            finallyRegions = FinallyRegionsProperty.ReadImmutableArray(instance, ControlFlowRegion.Wrap);
        }

        public static ControlFlowBranch Wrap(object instance) =>
            instance == null ? null : InstanceCache.GetValue(instance, x => new ControlFlowBranch(x));
    }
}
