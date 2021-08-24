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
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.CFG.Roslyn
{
    public class ControlFlowRegion
    {
        private static readonly PropertyInfo KindProperty;
        private static readonly PropertyInfo EnclosingRegionProperty;
        private static readonly PropertyInfo ExceptionTypeProperty;
        private static readonly PropertyInfo FirstBlockOrdinalProperty;
        private static readonly PropertyInfo LastBlockOrdinalProperty;
        private static readonly PropertyInfo NestedRegionsProperty;
        private static readonly PropertyInfo LocalsProperty;
        private static readonly PropertyInfo LocalFunctionsProperty;
        private static readonly PropertyInfo CaptureIdsProperty;
        private static readonly ConditionalWeakTable<object, ControlFlowRegion> InstanceCache = new ConditionalWeakTable<object, ControlFlowRegion>();

        private readonly Lazy<ControlFlowRegionKind> kind;
        private readonly Lazy<ControlFlowRegion> enclosingRegion;
        private readonly Lazy<ITypeSymbol> exceptionType;
        private readonly Lazy<int> firstBlockOrdinal;
        private readonly Lazy<int> lastBlockOrdinal;
        private readonly Lazy<ImmutableArray<ControlFlowRegion>> nestedRegions;
        private readonly Lazy<ImmutableArray<ILocalSymbol>> locals;
        private readonly Lazy<ImmutableArray<IMethodSymbol>> localFunctions;
        private readonly Lazy<ImmutableArray<CaptureId>> captureIds;

        public ControlFlowRegionKind Kind => kind.Value;
        public ControlFlowRegion EnclosingRegion => enclosingRegion.Value;
        public ITypeSymbol ExceptionType => exceptionType.Value;
        public int FirstBlockOrdinal => firstBlockOrdinal.Value;
        public int LastBlockOrdinal => lastBlockOrdinal.Value;
        public ImmutableArray<ControlFlowRegion> NestedRegions => nestedRegions.Value;
        public ImmutableArray<ILocalSymbol> Locals => locals.Value;
        public ImmutableArray<IMethodSymbol> LocalFunctions => localFunctions.Value;
        public ImmutableArray<CaptureId> CaptureIds => captureIds.Value;

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

        private ControlFlowRegion(object instance)
        {
            _ = instance ?? throw new ArgumentNullException(nameof(instance));
            kind = KindProperty.ReadValue<ControlFlowRegionKind>(instance);
            enclosingRegion = EnclosingRegionProperty.ReadValue(instance, Wrap);
            exceptionType = ExceptionTypeProperty.ReadValue<ITypeSymbol>(instance);
            firstBlockOrdinal = FirstBlockOrdinalProperty.ReadValue<int>(instance);
            lastBlockOrdinal = LastBlockOrdinalProperty.ReadValue<int>(instance);
            nestedRegions = NestedRegionsProperty.ReadImmutableArray(instance, Wrap);
            locals = LocalsProperty.ReadImmutableArray<ILocalSymbol>(instance);
            localFunctions = LocalFunctionsProperty.ReadImmutableArray<IMethodSymbol>(instance);
            captureIds = CaptureIdsProperty.ReadImmutableArray(instance, x => new CaptureId(x));
        }

        public static ControlFlowRegion Wrap(object instance) =>
            instance == null ? null : InstanceCache.GetValue(instance, x => new ControlFlowRegion(x));
    }
}
