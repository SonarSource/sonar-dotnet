/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class TokenTypeAnalyzer : TokenTypeAnalyzerBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = VisualBasicFacade.Instance;

        protected override TokenClassifierBase GetTokenClassifier(SemanticModel semanticModel, bool skipIdentifierTokens, string filePath) =>
            new TokenClassifier(semanticModel, skipIdentifierTokens, filePath);

        protected override TriviaClassifierBase GetTriviaClassifier(string filePath) =>
            new TriviaClassifier(filePath);

        private sealed class TokenClassifier : TokenClassifierBase
        {
            public TokenClassifier(SemanticModel semanticModel, bool skipIdentifiers, string filePath) : base(semanticModel, skipIdentifiers, filePath) { }

            protected override SyntaxNode GetBindableParent(SyntaxToken token) =>
                token.GetBindableParent();

            protected override bool IsIdentifier(SyntaxToken token) =>
                token.IsKind(SyntaxKind.IdentifierToken);

            protected override bool IsKeyword(SyntaxToken token) =>
                SyntaxFacts.IsKeywordKind(token.Kind());

            protected override bool IsNumericLiteral(SyntaxToken token) =>
                token.IsAnyKind(SyntaxKind.DecimalLiteralToken, SyntaxKind.FloatingLiteralToken, SyntaxKind.IntegerLiteralToken);

            protected override bool IsStringLiteral(SyntaxToken token) =>
                token.IsAnyKind(
                    SyntaxKind.StringLiteralToken,
                    SyntaxKind.CharacterLiteralToken,
                    SyntaxKind.InterpolatedStringTextToken,
                    SyntaxKind.EndOfInterpolatedStringToken);
        }

        private sealed class TriviaClassifier : TriviaClassifierBase
        {
            public TriviaClassifier(string filePath) : base(filePath) { }

            protected override bool IsRegularComment(SyntaxTrivia trivia) =>
                trivia.IsKind(SyntaxKind.CommentTrivia);

            protected override bool IsDocComment(SyntaxTrivia trivia) =>
                trivia.IsKind(SyntaxKind.DocumentationCommentTrivia);
        }
    }
}
