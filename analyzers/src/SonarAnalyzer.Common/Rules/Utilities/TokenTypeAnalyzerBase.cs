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
using static SonarAnalyzer.Protobuf.TokenTypeInfo.Types;

namespace SonarAnalyzer.Rules
{
    public abstract class TokenTypeAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, TokenTypeInfo>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S9999-token-type";
        private const string Title = "Token type calculator";
        private const int IdentifierTokenCountThreshold = 4_000;

        protected sealed override string FileName => "token-type.pb";

        protected TokenTypeAnalyzerBase() : base(DiagnosticId, Title) { }

        protected abstract TokenClassifierBase GetTokenClassifier(SemanticModel semanticModel, bool skipIdentifierTokens);
        protected abstract TriviaClassifierBase GetTriviaClassifier();

        protected sealed override TokenTypeInfo CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var filePath = GetFilePath(syntaxTree);
            var tokens = syntaxTree.GetRoot().DescendantTokens();
            var identifierTokenKind = Language.SyntaxKind.IdentifierToken;  // Performance optimization
            var skipIdentifierTokens = tokens
                .Where(token => Language.Syntax.IsKind(token, identifierTokenKind))
                .Skip(IdentifierTokenCountThreshold)
                .Any();

            var tokenClassifier = GetTokenClassifier(semanticModel, skipIdentifierTokens);
            var triviaClassifier = GetTriviaClassifier();
            var spans = new List<TokenInfo>();
            // The second iteration of the tokens is intended since there is no processing done and we want to avoid copying all the tokens to a second collection.
            foreach (var token in tokens)
            {
                if (token.HasLeadingTrivia)
                {
                    IterateTrivia(triviaClassifier, spans, token.LeadingTrivia, filePath);
                }
                if (tokenClassifier.ClassifyToken(token, filePath) is { } tokenClassification)
                {
                    spans.Add(tokenClassification);
                }
                if (token.HasTrailingTrivia)
                {
                    IterateTrivia(triviaClassifier, spans, token.TrailingTrivia, filePath);
                }
            }

            var tokenTypeInfo = new TokenTypeInfo
            {
                FilePath = filePath
            };

            tokenTypeInfo.TokenInfo.AddRange(spans);
            return tokenTypeInfo;

            static void IterateTrivia(TriviaClassifierBase triviaClassifier, List<TokenInfo> spans, SyntaxTriviaList triviaList, string filePath)
            {
                foreach (var trivia in triviaList)
                {
                    if (triviaClassifier.ClassifyTrivia(trivia, filePath) is { } triviaClassification)
                    {
                        spans.Add(triviaClassification);
                    }
                }
            }
        }

        protected abstract class TokenClassifierBase
        {
            private readonly SemanticModel semanticModel;
            private readonly bool skipIdentifiers;
            private static readonly ISet<MethodKind> ConstructorKinds = new HashSet<MethodKind>
            {
                MethodKind.Constructor,
                MethodKind.StaticConstructor,
                MethodKind.SharedConstructor
            };

            private static readonly ISet<SymbolKind> VarSymbolKinds = new HashSet<SymbolKind>
            {
                SymbolKind.NamedType,
                SymbolKind.TypeParameter,
                SymbolKind.ArrayType,
                SymbolKind.PointerType
            };

            protected abstract SyntaxNode GetBindableParent(SyntaxToken token);
            protected abstract bool IsKeyword(SyntaxToken token);
            protected abstract bool IsIdentifier(SyntaxToken token);
            protected abstract bool IsNumericLiteral(SyntaxToken token);
            protected abstract bool IsStringLiteral(SyntaxToken token);

            protected TokenClassifierBase(SemanticModel semanticModel, bool skipIdentifiers)
            {
                this.semanticModel = semanticModel;
                this.skipIdentifiers = skipIdentifiers;
            }

            public TokenInfo ClassifyToken(SyntaxToken token, string filePath) =>
                token switch
                {
                    _ when IsKeyword(token) => TokenInfo(token, TokenType.Keyword, filePath),
                    _ when IsStringLiteral(token) => TokenInfo(token, TokenType.StringLiteral, filePath),
                    _ when IsNumericLiteral(token) => TokenInfo(token, TokenType.NumericLiteral, filePath),
                    _ when IsIdentifier(token) && !skipIdentifiers => ClassifyIdentifier(token, filePath),
                    _ => null,
                };

            private TokenInfo ClassifyIdentifier(SyntaxToken token, string filePath)
            {
                if (semanticModel.GetDeclaredSymbol(token.Parent) is { } declaration)
                {
                    return ClassifyIdentifier(token, declaration, filePath);
                }
                else if (GetBindableParent(token) is { } parent && semanticModel.GetSymbolInfo(parent).Symbol is { } symbol)
                {
                    return ClassifyIdentifier(token, symbol, filePath);
                }
                else
                {
                    return null;
                }
            }

            private static TokenInfo ClassifyIdentifier(SyntaxToken token, ISymbol symbol, string filePath) =>
                symbol switch
                {
                    IAliasSymbol alias => ClassifyIdentifier(token, alias.Target, filePath),
                    IMethodSymbol ctorSymbol when ConstructorKinds.Contains(ctorSymbol.MethodKind) => TokenInfo(token, TokenType.TypeName, filePath),
                    _ when token.ValueText == "var" && VarSymbolKinds.Contains(symbol.Kind) => TokenInfo(token, TokenType.Keyword, filePath),
                    { Kind: SymbolKind.Parameter, IsImplicitlyDeclared: true } when token.ValueText == "value" => TokenInfo(token, TokenType.Keyword, filePath),
                    { Kind: SymbolKind.NamedType or SymbolKind.TypeParameter } => TokenInfo(token, TokenType.TypeName, filePath),
                    { Kind: SymbolKind.DynamicType } => TokenInfo(token, TokenType.Keyword, filePath),
                    _ => null,
                };

            private static TokenInfo TokenInfo(SyntaxToken token, TokenType tokenType, string filePath) =>
                string.IsNullOrWhiteSpace(token.ValueText)
                || !token.GetLocation().TryEnsureMappedLocation(out var mappedLocation)
                || !mappedLocation.GetLineSpan().Path.Equals(filePath, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : new()
                      {
                          TokenType = tokenType,
                          TextRange = GetTextRange(mappedLocation.GetLineSpan()),
                      };
        }

        protected abstract class TriviaClassifierBase
        {
            protected abstract bool IsDocComment(SyntaxTrivia trivia);
            protected abstract bool IsRegularComment(SyntaxTrivia trivia);

            public TokenInfo ClassifyTrivia(SyntaxTrivia trivia, string filePath) =>
                trivia switch
                {
                    _ when IsRegularComment(trivia) => TokenInfo(trivia, TokenType.Comment, filePath),
                    _ when IsDocComment(trivia) => ClassifyDocComment(trivia, filePath),
                    // Handle preprocessor directives here
                    _ => null,
                };

            private static TokenInfo ClassifyDocComment(SyntaxTrivia trivia, string filePath) =>
                TokenInfo(trivia, TokenType.Comment, filePath);

            private static TokenInfo TokenInfo(SyntaxTrivia trivia, TokenType tokenType, string filePath) =>
                trivia.GetLocation().TryEnsureMappedLocation(out var mappedLocation)
                && filePath.Equals(mappedLocation.GetLineSpan().Path, StringComparison.OrdinalIgnoreCase)
                    ? new TokenInfo { TokenType = tokenType, TextRange = GetTextRange(mappedLocation.GetLineSpan()) }
                    : null;
        }
    }
}
