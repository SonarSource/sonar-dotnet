/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    extension(ControlFlowRegion region)
    {
        public IEnumerable<BasicBlock> Blocks(ControlFlowGraph cfg) =>
            cfg.Blocks.Where((_, i) => region.FirstBlockOrdinal <= i && i <= region.LastBlockOrdinal);

        public ControlFlowRegion EnclosingNonLocalLifetimeRegion
        {
            get
            {
                while (region.EnclosingRegion is not null && region.Kind == ControlFlowRegionKind.LocalLifetime)
                {
                    region = region.EnclosingRegion;
                }
                return region;
            }
        }

        public ControlFlowRegion EnclosingRegionOrSelf(ControlFlowRegionKind kind)
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

        public ControlFlowRegion EnclosingRegion(ControlFlowRegionKind kind) =>
            region.EnclosingRegion.EnclosingRegionOrSelf(kind);

        public ControlFlowRegion NestedRegion(ControlFlowRegionKind kind) =>
            region.NestedRegions.Single(x => x.Kind == kind);

        /// <summary>
        /// Returns all Catch, FilterAndHandler, and Finally regions that are reachable from the given try region.
        /// </summary>
        public IEnumerable<ControlFlowRegion> ReachableHandlers =>
            region is null
                ? []
                : region.EnclosingRegion.NestedRegions.Where(x => x.Kind != ControlFlowRegionKind.Try)
                    .Concat(region.EnclosingRegion(ControlFlowRegionKind.Try).ReachableHandlers);   // Use also all outer candidates for nested try/catch.
    }
}
