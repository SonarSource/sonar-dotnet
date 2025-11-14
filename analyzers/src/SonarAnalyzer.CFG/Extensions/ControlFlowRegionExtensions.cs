/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.CFG.Extensions;

public static class ControlFlowRegionExtensions
{
    public static IEnumerable<BasicBlock> Blocks(this ControlFlowRegion region, ControlFlowGraph cfg) =>
        cfg.Blocks.Where((_, i) => region.FirstBlockOrdinal <= i && i <= region.LastBlockOrdinal);

    public static ControlFlowRegion EnclosingNonLocalLifetimeRegion(this ControlFlowRegion region)
    {
        while (region.EnclosingRegion is not null && region.Kind == ControlFlowRegionKind.LocalLifetime)
        {
            region = region.EnclosingRegion;
        }
        return region;
    }

    public static ControlFlowRegion EnclosingRegionOrSelf(this ControlFlowRegion region, ControlFlowRegionKind kind)
    {
        while (region is not null && region.Kind != kind)
        {
            if (region.Kind == ControlFlowRegionKind.Root)
            {
                return null;    // Do not traverse from inner lambda CFG to the outer method CFG
            }
            region = region.EnclosingRegion;
        }
        return region;
    }

    public static ControlFlowRegion EnclosingRegion(this ControlFlowRegion region, ControlFlowRegionKind kind) =>
        region.EnclosingRegion.EnclosingRegionOrSelf(kind);

    public static ControlFlowRegion NestedRegion(this ControlFlowRegion region, ControlFlowRegionKind kind) =>
        region.NestedRegions.Single(x => x.Kind == kind);

    /// <summary>
    /// Returns all Catch, FilterAndHandler, and Finally regions that are reachable from the given try region.
    /// </summary>
    public static IEnumerable<ControlFlowRegion> ReachableHandlers(this ControlFlowRegion tryRegion) =>
        tryRegion is null
            ? []
            : tryRegion.EnclosingRegion.NestedRegions.Where(x => x.Kind != ControlFlowRegionKind.Try)
                .Concat(ReachableHandlers(tryRegion.EnclosingRegion(ControlFlowRegionKind.Try)));   // Use also all outer candidates for nested try/catch.
}
