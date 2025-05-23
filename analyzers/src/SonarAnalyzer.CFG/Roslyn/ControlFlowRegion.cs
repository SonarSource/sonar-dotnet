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

using System.Reflection;
using System.Runtime.CompilerServices;
using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.CFG.Roslyn
{
    public class ControlFlowRegion
    {
        private static readonly ConditionalWeakTable<object, ControlFlowRegion> InstanceCache = new();
        private static readonly PropertyInfo KindProperty;
        private static readonly PropertyInfo EnclosingRegionProperty;
        private static readonly PropertyInfo ExceptionTypeProperty;
        private static readonly PropertyInfo FirstBlockOrdinalProperty;
        private static readonly PropertyInfo LastBlockOrdinalProperty;
        private static readonly PropertyInfo NestedRegionsProperty;
        private static readonly PropertyInfo LocalsProperty;
        private static readonly PropertyInfo LocalFunctionsProperty;
        private static readonly PropertyInfo CaptureIdsProperty;

        private readonly object instance;
        private ControlFlowRegionKind? kind;
        private ControlFlowRegion enclosingRegion;
        private ITypeSymbol exceptionType;
        private int? firstBlockOrdinal;
        private int? lastBlockOrdinal;
        private ImmutableArray<ControlFlowRegion> nestedRegions;
        private ImmutableArray<ILocalSymbol> locals;
        private ImmutableArray<IMethodSymbol> localFunctions;
        private ImmutableArray<CaptureId> captureIds;

        public ControlFlowRegionKind Kind => KindProperty.ReadCached(instance, ref kind);
        public ControlFlowRegion EnclosingRegion => EnclosingRegionProperty.ReadCached(instance, Wrap, ref enclosingRegion);
        public ITypeSymbol ExceptionType => ExceptionTypeProperty.ReadCached(instance, ref exceptionType);
        public int FirstBlockOrdinal => FirstBlockOrdinalProperty.ReadCached(instance, ref firstBlockOrdinal);
        public int LastBlockOrdinal => LastBlockOrdinalProperty.ReadCached(instance, ref lastBlockOrdinal);
        public ImmutableArray<ControlFlowRegion> NestedRegions => NestedRegionsProperty.ReadCached(instance, Wrap, ref nestedRegions);
        public ImmutableArray<ILocalSymbol> Locals => LocalsProperty.ReadCached(instance, ref locals);
        public ImmutableArray<IMethodSymbol> LocalFunctions => LocalFunctionsProperty.ReadCached(instance, ref localFunctions);
        public ImmutableArray<CaptureId> CaptureIds => CaptureIdsProperty.ReadCached(instance, x => new CaptureId(x), ref captureIds);

        static ControlFlowRegion()
        {
            if (RoslynHelper.FlowAnalysisType("ControlFlowRegion") is { } type)
            {
                KindProperty = type.GetProperty(nameof(Kind));
                EnclosingRegionProperty = type.GetProperty(nameof(EnclosingRegion));
                ExceptionTypeProperty = type.GetProperty(nameof(ExceptionType));
                FirstBlockOrdinalProperty = type.GetProperty(nameof(FirstBlockOrdinal));
                LastBlockOrdinalProperty = type.GetProperty(nameof(LastBlockOrdinal));
                NestedRegionsProperty = type.GetProperty(nameof(NestedRegions));
                LocalsProperty = type.GetProperty(nameof(Locals));
                LocalFunctionsProperty = type.GetProperty(nameof(LocalFunctions));
                CaptureIdsProperty = type.GetProperty(nameof(CaptureIds));
            }
        }

        private ControlFlowRegion(object instance) =>
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));

        public static ControlFlowRegion Wrap(object instance) =>
            instance == null ? null : InstanceCache.GetValue(instance, x => new ControlFlowRegion(x));
    }
}
