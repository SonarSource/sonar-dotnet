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

using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Protobuf;

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
            var tokens = syntaxTree.GetRoot().DescendantTokens();
            var identifierTokenKind = Language.SyntaxKind.IdentifierToken;  // Performance optimization
            var skipIdentifierTokens = tokens.Count(token => Language.Syntax.IsKind(token, identifierTokenKind)) > IdentifierTokenCountThreshold;

            var tokenClassifier = GetTokenClassifier(semanticModel, skipIdentifierTokens);
            var triviaClassifier = GetTriviaClassifier();
            var spans = new List<TokenTypeInfo.Types.TokenInfo>();
            // The second iteration of the tokens is intended since there is no processing done and we want to avoid copying all the tokens to a second collection.
            foreach (var token in tokens)
            {
                if (token.HasLeadingTrivia)
                {
                    IterateTrivia(token.LeadingTrivia);
                }
                if (tokenClassifier.ClassifyToken(token) is { } tokenClassification)
                {
                    spans.Add(tokenClassification);
                }
                if (token.HasTrailingTrivia)
                {
                    IterateTrivia(token.TrailingTrivia);
                }
            }

            var tokenTypeInfo = new TokenTypeInfo
            {
                FilePath = syntaxTree.FilePath
            };

            tokenTypeInfo.TokenInfo.AddRange(spans.OrderBy(s => s.TextRange.StartLine).ThenBy(s => s.TextRange.StartOffset));
            return tokenTypeInfo;

            void IterateTrivia(SyntaxTriviaList triviaList)
            {
                foreach (var trivia in triviaList)
                {
                    if (triviaClassifier.ClassifyTrivia(trivia) is { } triviaClassification)
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

            private TokenTypeInfo.Types.TokenInfo TokenInfo(SyntaxToken token, TokenType tokenType) =>
                string.IsNullOrWhiteSpace(token.ValueText)
                ? null
                : new()
                    {
                        TokenType = tokenType,
                        TextRange = GetTextRange(token.GetLocation().GetLineSpan()),
                    };

            public TokenTypeInfo.Types.TokenInfo ClassifyToken(SyntaxToken token)
            {
                if (IsKeyword(token))
                {
                    return TokenInfo(token, TokenType.Keyword);
                }
                else if (IsStringLiteral(token))
                {
                    return TokenInfo(token, TokenType.StringLiteral);
                }
                else if (IsNumericLiteral(token))
                {
                    return TokenInfo(token, TokenType.NumericLiteral);
                }
                else if (IsIdentifier(token) && !skipIdentifiers)
                {
                    return ClassifyIdentifier(token);
                }
                else
                {
                    return null;
                }
            }

            private TokenTypeInfo.Types.TokenInfo ClassifyIdentifier(SyntaxToken token)
            {
                if (semanticModel.GetDeclaredSymbol(token.Parent) is { } declaration)
                {
                    return ClassifyIdentifier(token, declaration);
                }
                else if (GetBindableParent(token) is { } parent && semanticModel.GetSymbolInfo(parent).Symbol is { } symbol)
                {
                    return ClassifyIdentifier(token, symbol);
                }
                else
                {
                    return null;
                }
            }

            private TokenTypeInfo.Types.TokenInfo ClassifyIdentifier(SyntaxToken token, ISymbol symbol)
            {
                if (symbol.Kind == SymbolKind.Alias)
                {
                    return ClassifyIdentifier(token, ((IAliasSymbol)symbol).Target);
                }
                else if (symbol is IMethodSymbol ctorSymbol && ConstructorKinds.Contains(ctorSymbol.MethodKind))
                {
                    return TokenInfo(token, TokenType.TypeName);
                }
                else if (token.ToString() == "var" && VarSymbolKinds.Contains(symbol.Kind))
                {
                    return TokenInfo(token, TokenType.Keyword);
                }
                else if (token.ToString() == "value" && symbol.Kind == SymbolKind.Parameter && symbol.IsImplicitlyDeclared)
                {
                    return TokenInfo(token, TokenType.Keyword);
                }
                else if (symbol.Kind == SymbolKind.NamedType || symbol.Kind == SymbolKind.TypeParameter)
                {
                    return TokenInfo(token, TokenType.TypeName);
                }
                else if (symbol.Kind == SymbolKind.DynamicType)
                {
                    return TokenInfo(token, TokenType.Keyword);
                }
                else
                {
                    return null;
                }
            }
        }

        protected abstract class TriviaClassifierBase
        {
            protected abstract bool IsDocComment(SyntaxTrivia trivia);
            protected abstract bool IsRegularComment(SyntaxTrivia trivia);

            public TokenTypeInfo.Types.TokenInfo ClassifyTrivia(SyntaxTrivia trivia)
            {
                if (IsRegularComment(trivia))
                {
                    return CollectClassified(trivia.SyntaxTree, TokenType.Comment, trivia.Span);
                }
                else if (IsDocComment(trivia))
                {
                    return ClassifyDocComment(trivia);
                }
                else
                {
                    return null;
                }
                // Handle preprocessor directives here
            }

            private TokenTypeInfo.Types.TokenInfo ClassifyDocComment(SyntaxTrivia trivia) =>
                CollectClassified(trivia.SyntaxTree, TokenType.Comment, trivia.FullSpan);

            private TokenTypeInfo.Types.TokenInfo CollectClassified(SyntaxTree tree, TokenType tokenType, TextSpan span) =>
                new()
                {
                    TokenType = tokenType,
                    TextRange = GetTextRange(Location.Create(tree, span).GetLineSpan())
                };
        }
    }
}
