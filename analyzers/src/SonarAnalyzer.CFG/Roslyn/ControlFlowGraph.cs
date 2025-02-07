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
using SonarAnalyzer.CFG.Common;

namespace SonarAnalyzer.CFG.Roslyn;

public class ControlFlowGraph
{
    private static readonly ConditionalWeakTable<object, ControlFlowGraph> InstanceCache = new();
    private static readonly PropertyInfo BlocksProperty;
    private static readonly PropertyInfo LocalFunctionsProperty;
    private static readonly PropertyInfo OriginalOperationProperty;
    private static readonly PropertyInfo ParentProperty;
    private static readonly PropertyInfo RootProperty;
    private static readonly MethodInfo CreateMethod;
    private static readonly MethodInfo GetAnonymousFunctionControlFlowGraphMethod;
    private static readonly MethodInfo GetLocalFunctionControlFlowGraphMethod;

    private readonly object instance;
    private ImmutableArray<BasicBlock> blocks;
    private ImmutableArray<IMethodSymbol> localFunctions;
    private ControlFlowGraph parent;
    private IOperation originalOperation;
    private ControlFlowRegion root;

    public static bool IsAvailable { get; }
    public ImmutableArray<BasicBlock> Blocks => BlocksProperty.ReadCached(instance, BasicBlock.Wrap, ref blocks);
    public ImmutableArray<IMethodSymbol> LocalFunctions => LocalFunctionsProperty.ReadCached<IMethodSymbol>(instance, ref localFunctions);
    public IOperation OriginalOperation => OriginalOperationProperty.ReadCached(instance, ref originalOperation);
    public ControlFlowGraph Parent => ParentProperty.ReadCached(instance, Wrap, ref parent);
    public ControlFlowRegion Root => RootProperty.ReadCached(instance, ControlFlowRegion.Wrap, ref root);
    public BasicBlock EntryBlock => Blocks[Root.FirstBlockOrdinal];
    public BasicBlock ExitBlock => Blocks[Root.LastBlockOrdinal];

    static ControlFlowGraph()
    {
        if (RoslynVersion.IsRoslynCfgSupported())
        {
            IsAvailable = true;
            var type = TypeLoader.FlowAnalysisType("ControlFlowGraph");
            BlocksProperty = type.GetProperty(nameof(Blocks));
            LocalFunctionsProperty = type.GetProperty(nameof(LocalFunctions));
            OriginalOperationProperty = type.GetProperty(nameof(OriginalOperation));
            ParentProperty = type.GetProperty(nameof(Parent));
            RootProperty = type.GetProperty(nameof(Root));
            CreateMethod = type.GetMethod(nameof(Create), new[] { typeof(SyntaxNode), typeof(SemanticModel), typeof(CancellationToken) });
            GetAnonymousFunctionControlFlowGraphMethod = type.GetMethod(nameof(GetAnonymousFunctionControlFlowGraph));
            GetLocalFunctionControlFlowGraphMethod = type.GetMethod(nameof(GetLocalFunctionControlFlowGraph));
        }
    }

    private ControlFlowGraph(object instance)
    {
        this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
        Debug.Assert(EntryBlock.Kind == BasicBlockKind.Entry, "Roslyn CFG Entry block is not the first one");
        Debug.Assert(ExitBlock.Kind == BasicBlockKind.Exit, "Roslyn CFG Exit block is not the last one");
    }

    public static ControlFlowGraph Create(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancel) =>
        IsAvailable
            ? Wrap(CreateMethod.Invoke(null, new object[] { node, semanticModel, cancel }))
            : throw new InvalidOperationException("CFG is not available under this version of Roslyn compiler.");

    public ControlFlowGraph GetAnonymousFunctionControlFlowGraph(IFlowAnonymousFunctionOperationWrapper anonymousFunction, CancellationToken cancel) =>
        GetAnonymousFunctionControlFlowGraphMethod.Invoke(instance, new object[] { anonymousFunction.WrappedOperation, cancel }) is { } anonymousFunctionCfg
            ? Wrap(anonymousFunctionCfg)
            : null;

    public ControlFlowGraph GetLocalFunctionControlFlowGraph(IMethodSymbol localFunction, CancellationToken cancel) =>
        GetLocalFunctionControlFlowGraphMethod.Invoke(instance, new object[] { localFunction, cancel }) is { } localFunctionCfg
            ? Wrap(localFunctionCfg)
            : null;

    public static ControlFlowGraph Wrap(object instance) =>
        instance == null ? null : InstanceCache.GetValue(instance, x => new ControlFlowGraph(x));
}
