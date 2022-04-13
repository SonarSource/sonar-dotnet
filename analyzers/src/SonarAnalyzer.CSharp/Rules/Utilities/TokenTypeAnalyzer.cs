/*
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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TokenTypeAnalyzer : TokenTypeAnalyzerBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = CSharpFacade.Instance;

        protected override TokenClassifierBase GetTokenClassifier(SyntaxToken token, SemanticModel semanticModel, bool skipIdentifierTokens) =>
            new TokenClassifier(token, semanticModel, skipIdentifierTokens);

        private sealed class TokenClassifier : TokenClassifierBase
        {
            public TokenClassifier(SyntaxToken token, SemanticModel semanticModel, bool skipIdentifiers) : base(token, semanticModel, skipIdentifiers) { }

            protected override SyntaxNode GetBindableParent(SyntaxToken token) =>
                token.GetBindableParent();

            protected override bool IsIdentifier(SyntaxToken token) =>
                token.IsKind(SyntaxKind.IdentifierToken);

            protected override bool IsKeyword(SyntaxToken token) =>
                SyntaxFacts.IsKeywordKind(token.Kind())
                || IsVarKeyword(token);

            private static bool IsVarKeyword(SyntaxToken token) =>
                token.Text == "var"
                && GetParent(token) is var parent
                && (parent is VariableDeclarationSyntax
                    || DeclarationExpressionSyntaxWrapper.IsInstance(parent)
                    || (parent is ForEachStatementSyntax forEachStatementSyntax && forEachStatementSyntax.Type == token.Parent)
                    || (parent is ForStatementSyntax forStatementSyntax && forStatementSyntax.Declaration.Type == token.Parent));

            protected override bool IsRegularComment(SyntaxTrivia trivia) =>
                trivia.IsAnyKind(SyntaxKind.SingleLineCommentTrivia, SyntaxKind.MultiLineCommentTrivia);

            protected override bool IsNumericLiteral(SyntaxToken token) =>
                token.IsKind(SyntaxKind.NumericLiteralToken);

            protected override bool IsStringLiteral(SyntaxToken token) =>
                token.IsAnyKind(
                    SyntaxKind.StringLiteralToken,
                    SyntaxKind.CharacterLiteralToken,
                    SyntaxKind.InterpolatedStringStartToken,
                    SyntaxKind.InterpolatedVerbatimStringStartToken,
                    SyntaxKind.InterpolatedStringTextToken,
                    SyntaxKind.InterpolatedStringEndToken);

            protected override bool IsTypeName(SyntaxToken token) =>
                GetParent(token) is BaseTypeDeclarationSyntax
                    or ConstructorDeclarationSyntax
                    or ObjectCreationExpressionSyntax
                    or TypeParameterSyntax
                    or GenericNameSyntax
                    or AttributeSyntax;

            private static SyntaxNode GetParent(SyntaxToken token)
            {
                var parent = token.Parent;
                while (parent is IdentifierNameSyntax or QualifiedNameSyntax)
                {
                    parent = parent.Parent;
                }
                return parent;
            }

            protected override bool IsDocComment(SyntaxTrivia trivia) =>
                trivia.IsAnyKind(SyntaxKind.SingleLineDocumentationCommentTrivia, SyntaxKind.MultiLineDocumentationCommentTrivia);
        }
    }
}
