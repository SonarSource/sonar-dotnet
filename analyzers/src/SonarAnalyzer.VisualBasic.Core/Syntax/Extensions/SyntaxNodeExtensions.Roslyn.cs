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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.VisualBasic.Extensions;

[ExcludeFromCodeCoverage]
internal static class SyntaxNodeExtensions
{
    // Copied and converted from
    // https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/VisualBasic/Extensions/SyntaxNodeExtensions.vb#L16
    public static bool IsParentKind(this SyntaxNode node, SyntaxKind kind)
    {
        return node != null && node.Parent.IsKind(kind);
    }
}
