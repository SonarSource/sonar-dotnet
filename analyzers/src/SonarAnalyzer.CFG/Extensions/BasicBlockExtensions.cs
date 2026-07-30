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

public static class BasicBlockExtensions
{
    extension(BasicBlock block)
    {
        public bool IsEnclosedIn(ControlFlowRegionKind kind)
        {
            var enclosing = kind == ControlFlowRegionKind.LocalLifetime ? block.EnclosingRegion : block.EnclosingNonLocalLifetimeRegion;
            return enclosing.Kind == kind;
        }

        public ControlFlowRegion EnclosingNonLocalLifetimeRegion => block.EnclosingRegion.EnclosingNonLocalLifetimeRegion;

        public ControlFlowRegion EnclosingRegion(ControlFlowRegionKind kind) =>
            block.EnclosingRegion.EnclosingRegionOrSelf(kind);
    }
}
