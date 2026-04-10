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

    public static ComparisonKind ToComparisonKind(this SyntaxToken token) =>
        token.Kind() switch
        {
            SyntaxKind.EqualsEqualsToken => ComparisonKind.Equals,
            SyntaxKind.ExclamationEqualsToken => ComparisonKind.NotEquals,
            SyntaxKind.LessThanToken => ComparisonKind.LessThan,
            SyntaxKind.LessThanEqualsToken => ComparisonKind.LessThanOrEqual,
            SyntaxKind.GreaterThanToken => ComparisonKind.GreaterThan,
            SyntaxKind.GreaterThanEqualsToken => ComparisonKind.GreaterThanOrEqual,
            _ => ComparisonKind.None,
        };
}
