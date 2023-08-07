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

using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TokenTypeAnalyzer : TokenTypeAnalyzerBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = CSharpFacade.Instance;

        protected override TokenClassifierBase GetTokenClassifier(SemanticModel semanticModel, bool skipIdentifierTokens) =>
            new TokenClassifier(semanticModel, skipIdentifierTokens);

        protected override TriviaClassifierBase GetTriviaClassifier() =>
            new TriviaClassifier();

        internal sealed class TokenClassifier : TokenClassifierBase
        {
            private static readonly SyntaxKind[] StringLiteralTokens = new[]
            {
                SyntaxKind.StringLiteralToken,
                SyntaxKind.CharacterLiteralToken,
                SyntaxKindEx.SingleLineRawStringLiteralToken,
                SyntaxKindEx.MultiLineRawStringLiteralToken,
                SyntaxKindEx.Utf8StringLiteralToken,
                SyntaxKindEx.Utf8SingleLineRawStringLiteralToken,
                SyntaxKindEx.Utf8MultiLineRawStringLiteralToken,
                SyntaxKind.InterpolatedStringStartToken,
                SyntaxKind.InterpolatedVerbatimStringStartToken,
                SyntaxKindEx.InterpolatedSingleLineRawStringStartToken,
                SyntaxKindEx.InterpolatedMultiLineRawStringStartToken,
                SyntaxKind.InterpolatedStringTextToken,
                SyntaxKind.InterpolatedStringEndToken,
                SyntaxKindEx.InterpolatedRawStringEndToken,
            };

            public TokenClassifier(SemanticModel semanticModel, bool skipIdentifiers) : base(semanticModel, skipIdentifiers) { }

            protected override SyntaxNode GetBindableParent(SyntaxToken token) =>
                token.GetBindableParent();

            protected override bool IsIdentifier(SyntaxToken token) =>
                token.IsKind(SyntaxKind.IdentifierToken);

            protected override bool IsKeyword(SyntaxToken token) =>
                SyntaxFacts.IsKeywordKind(token.Kind());

            protected override bool IsNumericLiteral(SyntaxToken token) =>
                token.IsKind(SyntaxKind.NumericLiteralToken);

            protected override bool IsStringLiteral(SyntaxToken token) =>
                token.IsAnyKind(StringLiteralTokens);

            protected override TokenTypeInfo.Types.TokenInfo ClassifyIdentifier(SyntaxToken token) =>
                // Based on <Kind Name="IdentifierToken"/> in SonarAnalyzer.CFG/ShimLayer\Syntax.xml
                token.Parent switch
                {
                    FromClauseSyntax x when token == x.Identifier => null,
                    LetClauseSyntax x when token == x.Identifier => null,
                    JoinClauseSyntax x when token == x.Identifier => null,
                    JoinIntoClauseSyntax x when token == x.Identifier => null,
                    QueryContinuationSyntax x when token == x.Identifier => null,
                    VariableDeclaratorSyntax x when token == x.Identifier => null,
                    LabeledStatementSyntax x when token == x.Identifier => null,
                    ForEachStatementSyntax x when token == x.Identifier => null,
                    CatchDeclarationSyntax x when token == x.Identifier => null,
                    ExternAliasDirectiveSyntax x when token == x.Identifier => null,
                    EnumMemberDeclarationSyntax x when token == x.Identifier => null,
                    MethodDeclarationSyntax x when token == x.Identifier => null,
                    PropertyDeclarationSyntax x when token == x.Identifier => null,
                    EventDeclarationSyntax x when token == x.Identifier => null,
                    AccessorDeclarationSyntax x when token == x.Keyword => null,
                    ParameterSyntax x when token == x.Identifier => null,
                    var x when FunctionPointerUnmanagedCallingConventionSyntaxWrapper.IsInstance(x) && token == ((FunctionPointerUnmanagedCallingConventionSyntaxWrapper)x).Name => null,
                    var x when TupleElementSyntaxWrapper.IsInstance(x) && token == ((TupleElementSyntaxWrapper)x).Identifier => null,
                    var x when LocalFunctionStatementSyntaxWrapper.IsInstance(x) && token == ((LocalFunctionStatementSyntaxWrapper)x).Identifier => null,
                    var x when SingleVariableDesignationSyntaxWrapper.IsInstance(x) && token == ((SingleVariableDesignationSyntaxWrapper)x).Identifier => null,
                    TypeParameterSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    BaseTypeDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    DelegateDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    ConstructorDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    DestructorDeclarationSyntax x when token == x.Identifier => TokenInfo(token, TokenType.TypeName),
                    AttributeTargetSpecifierSyntax x when token == x.Identifier => TokenInfo(token, TokenType.Keyword), // for unknown target specifier [unknown: Obsolete]
                    IdentifierNameSyntax x when IsInUsingWithNoTypes(x) => null,
                    _ => base.ClassifyIdentifier(token),
                };

            private static bool IsInUsingWithNoTypes(IdentifierNameSyntax identifier)
            {
                var parent = identifier.Parent;
                while (parent is NameSyntax)
                {
                    parent = parent.Parent;
                }
                return parent is UsingDirectiveSyntax usingSyntax
                       && usingSyntax.StaticKeyword == default
                       && usingSyntax.Alias is null;
            }
        }

        internal sealed class TriviaClassifier : TriviaClassifierBase
        {
            private static readonly SyntaxKind[] RegularCommentToken = new[]
            {
                SyntaxKind.SingleLineCommentTrivia,
                SyntaxKind.MultiLineCommentTrivia,
            };

            private static readonly SyntaxKind[] DocCommentToken = new[]
            {
                SyntaxKind.SingleLineDocumentationCommentTrivia,
                SyntaxKind.MultiLineDocumentationCommentTrivia,
            };

            protected override bool IsRegularComment(SyntaxTrivia trivia) =>
                trivia.IsAnyKind(RegularCommentToken);

            protected override bool IsDocComment(SyntaxTrivia trivia) =>
                trivia.IsAnyKind(DocCommentToken);
        }
    }
}
