/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using SonarAnalyzer.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class TokenTypeAnalyzer : TokenTypeAnalyzerBase
    {
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.VisualBasic.GeneratedCodeRecognizer.Instance;

        protected override TokenClassifierBase GetTokenClassifier(SyntaxToken token, SemanticModel semanticModel)
            => new TokenClassifier(token, semanticModel);

        private class TokenClassifier : TokenClassifierBase
        {
            public TokenClassifier(SyntaxToken token, SemanticModel semanticModel)
                : base(token, semanticModel)
            {
            }

            protected override SyntaxNode GetBindableParent(SyntaxToken token)
            {
                return new SymbolReferenceAnalyzer().GetBindableParent(token);
            }

            protected override bool IsIdentifier(SyntaxToken token)
            {
                return token.IsKind(SyntaxKind.IdentifierToken);
            }

            protected override bool IsKeyword(SyntaxToken token)
            {
                return SyntaxFacts.IsKeywordKind(token.Kind());
            }

            protected override bool IsContextualKeyword(SyntaxToken token)
            {
                return SyntaxFacts.IsContextualKeyword(token.Kind());
            }

            protected override bool IsRegularComment(SyntaxTrivia trivia)
            {
                return trivia.IsKind(SyntaxKind.CommentTrivia);
            }

            protected override bool IsNumericLiteral(SyntaxToken token)
            {
                return NumericKinds.Contains(token.Kind());
            }

            protected override bool IsStringLiteral(SyntaxToken token)
            {
                return StringKinds.Contains(token.Kind());
            }

            protected override bool IsDocComment(SyntaxTrivia trivia)
            {
                return trivia.IsKind(SyntaxKind.DocumentationCommentTrivia);
            }

            private static readonly ISet<SyntaxKind> StringKinds = ImmutableHashSet.Create(
                SyntaxKind.StringLiteralToken,
                SyntaxKind.CharacterLiteralToken,
                SyntaxKind.InterpolatedStringTextToken,
                SyntaxKind.EndOfInterpolatedStringToken);


            private static readonly ISet<SyntaxKind> NumericKinds = ImmutableHashSet.Create(
                SyntaxKind.DecimalLiteralToken,
                SyntaxKind.FloatingLiteralToken,
                SyntaxKind.IntegerLiteralToken);
        }
    }
}
