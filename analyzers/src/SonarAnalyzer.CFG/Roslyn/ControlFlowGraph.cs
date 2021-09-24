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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.CFG.Roslyn
{
    public class ControlFlowGraph
    {
        private static readonly ConditionalWeakTable<object, ControlFlowGraph> InstanceCache = new ConditionalWeakTable<object, ControlFlowGraph>();
        private static readonly PropertyInfo BlocksProperty;
        private static readonly PropertyInfo LocalFunctionsProperty;
        private static readonly PropertyInfo OriginalOperationProperty;
        private static readonly PropertyInfo ParentProperty;
        private static readonly PropertyInfo RootProperty;
        private static readonly MethodInfo CreateMethod;
        private static readonly MethodInfo GetAnonymousFunctionControlFlowGraphMethod;
        private static readonly MethodInfo GetLocalFunctionControlFlowGraphMethod;

        private readonly object instance;
        private readonly Lazy<ImmutableArray<BasicBlock>> blocks;
        private readonly Lazy<ImmutableArray<IMethodSymbol>> localFunctions;
        private readonly Lazy<ControlFlowGraph> parent;
        private readonly Lazy<IOperation> originalOperation;
        private readonly Lazy<ControlFlowRegion> root;

        public static bool IsAvailable { get; }
        public ImmutableArray<BasicBlock> Blocks => blocks.Value;
        public ImmutableArray<IMethodSymbol> LocalFunctions => localFunctions.Value;
        public IOperation OriginalOperation => originalOperation.Value;
        public ControlFlowGraph Parent => parent.Value;
        public ControlFlowRegion Root => root.Value;
        public BasicBlock EntryBlock => Blocks[Root.FirstBlockOrdinal];
        public BasicBlock ExitBlock => Blocks[Root.LastBlockOrdinal];

        static ControlFlowGraph()
        {
            if (RoslynHelper.IsRoslynCfgSupported())
            {
                IsAvailable = true;
                var type = RoslynHelper.FlowAnalysisType("ControlFlowGraph");
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
            blocks = BlocksProperty.ReadImmutableArray(instance, BasicBlock.Wrap);
            localFunctions = LocalFunctionsProperty.ReadImmutableArray<IMethodSymbol>(instance);
            originalOperation = OriginalOperationProperty.ReadValue<IOperation>(instance);
            parent = ParentProperty.ReadValue(instance, Wrap);
            root = RootProperty.ReadValue(instance, ControlFlowRegion.Wrap);
            Debug.Assert(EntryBlock.Kind == BasicBlockKind.Entry, "Roslyn CFG Entry block is not the first one");
            Debug.Assert(ExitBlock.Kind == BasicBlockKind.Exit, "Roslyn CFG Exit block is not the last one");
        }

        public static ControlFlowGraph Create(SyntaxNode node, SemanticModel semanticModel) =>
            IsAvailable
                ? Wrap(CreateMethod.Invoke(null, new object[] { node, semanticModel, CancellationToken.None }))
                : throw new InvalidOperationException("CFG is not available under this version of Roslyn compiler.");

        public ControlFlowGraph GetAnonymousFunctionControlFlowGraph(IFlowAnonymousFunctionOperationWrapper anonymousFunction) =>
            GetAnonymousFunctionControlFlowGraphMethod.Invoke(instance, new object[] { anonymousFunction.WrappedOperation, CancellationToken.None }) is { } anonymousFunctionCfg
                ? Wrap(anonymousFunctionCfg)
                : null;

        public ControlFlowGraph GetLocalFunctionControlFlowGraph(IMethodSymbol localFunction) =>
            GetLocalFunctionControlFlowGraphMethod.Invoke(instance, new object[] { localFunction, CancellationToken.None }) is { } localFunctionCfg
                ? Wrap(localFunctionCfg)
                : null;

        public static ControlFlowGraph Wrap(object instance) =>
            instance == null ? null : InstanceCache.GetValue(instance, x => new ControlFlowGraph(x));
    }
}
