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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.CFG.Roslyn
{
    public class ControlFlowGraph
    {
        private static readonly MethodInfo CreateMethod;
        private static readonly PropertyInfo RootProperty;
        private static readonly PropertyInfo BlocksProperty;

        private readonly Lazy<ControlFlowRegion> root;
        private readonly Lazy<ImmutableArray<BasicBlock>> blocks;

        public static bool IsAvailable { get; }
        public ControlFlowRegion Root => root.Value;
        public ImmutableArray<BasicBlock> Blocks => blocks.Value;

        static ControlFlowGraph()
        {
            var type = RoslynHelper.FlowAnalysisType("ControlFlowGraph");
            if (type != null)
            {
                IsAvailable = true;
                CreateMethod = type.GetMethod(nameof(Create), new[] { typeof(SyntaxNode), typeof(SemanticModel), typeof(CancellationToken) });
                RootProperty = type.GetProperty(nameof(Root));
                BlocksProperty = type.GetProperty(nameof(Blocks));

            }
        }

        private ControlFlowGraph(object instance)
        {
            _ = instance ?? throw new ArgumentNullException(nameof(instance));
            root = new Lazy<ControlFlowRegion>(() => new ControlFlowRegion(RootProperty.GetValue(instance)));
            blocks = new Lazy<ImmutableArray<BasicBlock>>(() => ((IEnumerable<object>)BlocksProperty.GetValue(instance)).Select(x => new BasicBlock(x)).ToImmutableArray());
        }

        public static ControlFlowGraph Create(SyntaxNode node, SemanticModel semanticModel) =>
            IsAvailable
                ? new ControlFlowGraph(CreateMethod.Invoke(null, new object[] { node, semanticModel, CancellationToken.None }))
                : throw new InvalidOperationException("CFG is not available under this version of Roslyn compiler.");
    }
}
