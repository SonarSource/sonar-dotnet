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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TokenTypeAnalyzer : TokenTypeAnalyzerBase
    {
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer =>
            Helpers.CSharp.CSharpGeneratedCodeRecognizer.Instance;

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

            protected override bool IsRegularComment(SyntaxTrivia trivia)
            {
                switch (trivia.Kind())
                {
                    case SyntaxKind.SingleLineCommentTrivia:
                    case SyntaxKind.MultiLineCommentTrivia:
                        return true;

                    default:
                        return false;
                }
            }

            protected override bool IsNumericLiteral(SyntaxToken token) =>
                token.IsKind(SyntaxKind.NumericLiteralToken);

            protected override bool IsStringLiteral(SyntaxToken token)
            {
                switch (token.Kind())
                {
                    case SyntaxKind.StringLiteralToken:
                    case SyntaxKind.CharacterLiteralToken:
                    case SyntaxKind.InterpolatedStringStartToken:
                    case SyntaxKind.InterpolatedVerbatimStringStartToken:
                    case SyntaxKind.InterpolatedStringTextToken:
                    case SyntaxKind.InterpolatedStringEndToken:
                        return true;

                    default:
                        return false;
                }
            }

            protected override bool IsDocComment(SyntaxTrivia trivia) =>
                trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
        }
    }
}
