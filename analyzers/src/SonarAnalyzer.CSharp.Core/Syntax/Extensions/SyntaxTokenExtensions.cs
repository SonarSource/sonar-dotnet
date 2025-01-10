/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class SyntaxTokenExtensions
{
    [Obsolete("Either use '.Kind() is A or B' or the overload with the ISet instead.")]
    public static bool IsAnyKind(this SyntaxToken token, params SyntaxKind[] syntaxKinds) =>
        syntaxKinds.Contains((SyntaxKind)token.RawKind);

    public static bool IsAnyKind(this SyntaxToken token, ISet<SyntaxKind> syntaxKinds) =>
        syntaxKinds.Contains((SyntaxKind)token.RawKind);

    public static bool AnyOfKind(this IEnumerable<SyntaxToken> tokens, SyntaxKind kind) =>
        tokens.Any(x => x.RawKind == (int)kind);
}
