﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using SonarAnalyzer.Protobuf;
using SonarAnalyzer.ShimLayer;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TokenTypeAnalyzer : TokenTypeAnalyzerBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = CSharpFacade.Instance;

        protected override string ClassifyToken(SyntaxToken token) =>
            ClassificationHelpersWrapper.GetCSharpClassification(token);

        protected override TokenType ClassifyTrivia(SyntaxTrivia trivia) =>
            trivia.Kind() switch
            {
                SyntaxKind.SingleLineCommentTrivia
                or SyntaxKind.MultiLineCommentTrivia
                or SyntaxKind.SingleLineDocumentationCommentTrivia
                or SyntaxKind.MultiLineDocumentationCommentTrivia => TokenType.Comment,
                _ => TokenType.UnknownTokentype,
            };

        private static SyntaxNode WalkUpNames(SyntaxNode node)
        {
            while (node is TypeSyntax)
            {
                node = node.Parent;
            }
            return node;
        }

        protected override bool IsTypeIdentifier(SyntaxToken token) =>
            WalkUpNames(token.Parent) is ObjectCreationExpressionSyntax
            // TODO: Fix token in the middle of qualified name
            && token is { Parent.Parent: not QualifiedNameSyntax };
    }
}
