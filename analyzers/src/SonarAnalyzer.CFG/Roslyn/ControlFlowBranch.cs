/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CFG.Roslyn;

public class ControlFlowBranch
{
    private static readonly ConditionalWeakTable<object, ControlFlowBranch> InstanceCache = new();
    private static readonly PropertyInfo SourceProperty;
    private static readonly PropertyInfo DestinationProperty;
    private static readonly PropertyInfo SemanticsProperty;
    private static readonly PropertyInfo IsConditionalSuccessorProperty;
    private static readonly PropertyInfo EnteringRegionsProperty;
    private static readonly PropertyInfo LeavingRegionsProperty;
    private static readonly PropertyInfo FinallyRegionsProperty;

    private readonly object instance;
    private BasicBlock source;
    private BasicBlock destination;
    private ControlFlowBranchSemantics? semantics;
    private bool? isConditionalSuccessor;
    private ImmutableArray<ControlFlowRegion> enteringRegions;
    private ImmutableArray<ControlFlowRegion> leavingRegions;
    private ImmutableArray<ControlFlowRegion> finallyRegions;

    public BasicBlock Source => SourceProperty.ReadCached(instance, BasicBlock.Wrap, ref source);
    public BasicBlock Destination => DestinationProperty.ReadCached(instance, BasicBlock.Wrap, ref destination);
    public ControlFlowBranchSemantics Semantics => SemanticsProperty.ReadCached(instance, ref semantics);
    public bool IsConditionalSuccessor => IsConditionalSuccessorProperty.ReadCached(instance, ref isConditionalSuccessor);
    public ImmutableArray<ControlFlowRegion> EnteringRegions => EnteringRegionsProperty.ReadCached(instance, ControlFlowRegion.Wrap, ref enteringRegions);
    public ImmutableArray<ControlFlowRegion> LeavingRegions => LeavingRegionsProperty.ReadCached(instance, ControlFlowRegion.Wrap, ref leavingRegions);
    public ImmutableArray<ControlFlowRegion> FinallyRegions => FinallyRegionsProperty.ReadCached(instance, ControlFlowRegion.Wrap, ref finallyRegions);

    static ControlFlowBranch()
    {
        if (TypeLoader.FlowAnalysisType("ControlFlowBranch") is { } type)
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

    private ControlFlowBranch(object instance) =>
        this.instance = instance ?? throw new ArgumentNullException(nameof(instance));

    public static ControlFlowBranch Wrap(object instance) =>
        instance == null ? null : InstanceCache.GetValue(instance, x => new ControlFlowBranch(x));
}
