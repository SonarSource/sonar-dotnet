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

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.CFG.Extensions;

public static class BasicBlockExtensions
{
    public static bool IsEnclosedIn(this BasicBlock block, ControlFlowRegionKind kind)
    {
        var enclosing = kind == ControlFlowRegionKind.LocalLifetime ? block.EnclosingRegion : block.EnclosingNonLocalLifetimeRegion();
        return enclosing.Kind == kind;
    }

    public static ControlFlowRegion EnclosingNonLocalLifetimeRegion(this BasicBlock block) =>
        block.EnclosingRegion.EnclosingNonLocalLifetimeRegion();

    public static ControlFlowRegion EnclosingRegion(this BasicBlock block, ControlFlowRegionKind kind) =>
        block.EnclosingRegion.EnclosingRegionOrSelf(kind);
}
