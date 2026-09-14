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

public static class ControlFlowRegionWrapperExtensions
{
    extension(ControlFlowRegionWrapper region)
    {
        public ControlFlowRegionWrapper EnclosingNonLocalLifetimeRegion
        {
            get
            {
                while (region.EnclosingRegion.WrappedInstance is not null && region.Kind == ControlFlowRegionKind.LocalLifetime)
                {
                    region = region.EnclosingRegion;
                }
                return region;
            }
        }

        /// <summary>
        /// Returns all Catch, FilterAndHandler, and Finally regions that are reachable from the given try region.
        /// </summary>
        public IEnumerable<ControlFlowRegionWrapper> ReachableHandlers =>
            region.WrappedInstance is null
                ? []
                : region.EnclosingRegion.NestedRegions.Where(x => x.Kind != ControlFlowRegionKind.Try)
                    .Concat(region.EnclosingRegion(ControlFlowRegionKind.Try)?.ReachableHandlers ?? []);    // Use also all outer candidates for nested try/catch.

        public IEnumerable<BasicBlock> Blocks(ControlFlowGraph cfg) =>
            cfg.Blocks.Where((_, i) => region.FirstBlockOrdinal <= i && i <= region.LastBlockOrdinal);

        public ControlFlowRegionWrapper? EnclosingRegionOrSelf(ControlFlowRegionKind kind)
        {
            while (region.WrappedInstance is not null && region.Kind != kind)
            {
                if (region.Kind == ControlFlowRegionKind.Root)
                {
                    return null;    // Do not traverse from inner lambda CFG to the outer method CFG
                }
                region = region.EnclosingRegion;
            }
            return region.WrappedInstance is null ? null : region;
        }

        public ControlFlowRegionWrapper? EnclosingRegion(ControlFlowRegionKind kind) =>
            region.EnclosingRegion.EnclosingRegionOrSelf(kind);

        public ControlFlowRegionWrapper NestedRegion(ControlFlowRegionKind kind) =>
            region.NestedRegions.Single(x => x.Kind == kind);
    }
}
