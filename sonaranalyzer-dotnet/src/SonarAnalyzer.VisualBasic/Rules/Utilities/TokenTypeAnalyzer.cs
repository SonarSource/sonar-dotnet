/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class TokenTypeAnalyzer : TokenTypeAnalyzerBase
    {
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer =>
            Helpers.VisualBasic.GeneratedCodeRecognizer.Instance;

        protected override TokenClassifierBase GetTokenClassifier(SyntaxToken token, SemanticModel semanticModel) =>
            new TokenClassifier(token, semanticModel);

        private class TokenClassifier : TokenClassifierBase
        {
            public TokenClassifier(SyntaxToken token, SemanticModel semanticModel)
                : base(token, semanticModel)
            {
            }

            protected override SyntaxNode GetBindableParent(SyntaxToken token) =>
                new SymbolReferenceAnalyzer().GetBindableParent(token);

            protected override bool IsIdentifier(SyntaxToken token) =>
                token.IsKind(SyntaxKind.IdentifierToken);

            protected override bool IsKeyword(SyntaxToken token) =>
                SyntaxFacts.IsKeywordKind(token.Kind());

            protected override bool IsContextualKeyword(SyntaxToken token) =>
                SyntaxFacts.IsContextualKeyword(token.Kind());

            protected override bool IsRegularComment(SyntaxTrivia trivia) =>
                trivia.IsKind(SyntaxKind.CommentTrivia);

            protected override bool IsNumericLiteral(SyntaxToken token)
            {
                switch (token.Kind())
                {
                    case SyntaxKind.DecimalLiteralToken:
                    case SyntaxKind.FloatingLiteralToken:
                    case SyntaxKind.IntegerLiteralToken:
                        return true;

                    default:
                        return false;
                }
            }

            protected override bool IsStringLiteral(SyntaxToken token)
            {
                switch (token.Kind())
                {
                    case SyntaxKind.StringLiteralToken:
                    case SyntaxKind.CharacterLiteralToken:
                    case SyntaxKind.InterpolatedStringTextToken:
                    case SyntaxKind.EndOfInterpolatedStringToken:
                        return true;

                    default:
                        return false;
                }
            }

            protected override bool IsDocComment(SyntaxTrivia trivia) =>
                trivia.IsKind(SyntaxKind.DocumentationCommentTrivia);
        }
    }
}
