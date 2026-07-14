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

namespace SonarAnalyzer.CSharp.Core.Extensions;

public static class StringExtensions
{
    extension(string name)
    {
        /// <summary>
        /// Returns an <see cref="IdentifierNameSyntax"/> for this name, adding the <c>@</c> verbatim prefix
        /// when the name is a reserved or contextual keyword (e.g. <c>"this"</c> → <c>@this</c>).
        /// Use this instead of <see cref="SyntaxFactory.IdentifierName(string)"/> when the name is a bare
        /// identifier string that may be a keyword.
        /// </summary>
        public IdentifierNameSyntax EscapedIdentifierName =>
            SyntaxFactory.IdentifierName(
                SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None && SyntaxFacts.GetContextualKeywordKind(name) == SyntaxKind.None
                    ? SyntaxFactory.Identifier(name)
                    : SyntaxFactory.VerbatimIdentifier(SyntaxTriviaList.Empty, name, name, SyntaxTriviaList.Empty));
    }
}
