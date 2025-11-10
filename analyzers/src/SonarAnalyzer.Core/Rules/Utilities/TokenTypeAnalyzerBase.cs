/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Protobuf;
using static SonarAnalyzer.Protobuf.TokenTypeInfo.Types;

namespace SonarAnalyzer.Core.Rules
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

        protected sealed override bool ShouldGenerateMetrics(UtilityAnalyzerParameters parameters, SyntaxTree tree) =>
            !GeneratedCodeRecognizer.IsRazorGeneratedFile(tree)
            && base.ShouldGenerateMetrics(parameters, tree);

        protected sealed override TokenTypeInfo CreateMessage(UtilityAnalyzerParameters parameters, SyntaxTree tree, SemanticModel model)
        {
            var tokens = tree.GetRoot().DescendantTokens();
            var identifierTokenKind = Language.SyntaxKind.IdentifierToken;  // Performance optimization
            var skipIdentifierTokens = tokens
                .Where(token => Language.Syntax.IsKind(token, identifierTokenKind))
                .Skip(IdentifierTokenCountThreshold)
                .Any();

            var tokenClassifier = GetTokenClassifier(model, skipIdentifierTokens);
            var triviaClassifier = GetTriviaClassifier();
            var spans = new List<TokenInfo>();
            // The second iteration of the tokens is intended since there is no processing done and we want to avoid copying all the tokens to a second collection.
            foreach (var token in tokens)
            {
                if (token.HasLeadingTrivia)
                {
                    IterateTrivia(triviaClassifier, spans, token.LeadingTrivia);
                }
                if (tokenClassifier.ClassifyToken(token) is { } tokenClassification)
                {
                    spans.Add(tokenClassification);
                }
                if (token.HasTrailingTrivia)
                {
                    IterateTrivia(triviaClassifier, spans, token.TrailingTrivia);
                }
            }

            var tokenTypeInfo = new TokenTypeInfo
            {
                FilePath = tree.FilePath
            };

            tokenTypeInfo.TokenInfo.AddRange(spans);
            return tokenTypeInfo;

            static void IterateTrivia(TriviaClassifierBase triviaClassifier, List<TokenInfo> spans, SyntaxTriviaList triviaList)
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

        protected internal abstract class TokenClassifierBase
        {
            private readonly bool skipIdentifiers;
            private readonly SemanticModel semanticModel;

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

            protected SemanticModel SemanticModel => semanticModel ?? throw new InvalidOperationException("The code snippet is not supposed to call the semantic model for classification.");

            protected TokenClassifierBase(SemanticModel semanticModel, bool skipIdentifiers)
            {
                this.semanticModel = semanticModel;
                this.skipIdentifiers = skipIdentifiers;
            }

            public TokenInfo ClassifyToken(SyntaxToken token) =>
                token switch
                {
                    _ when IsKeyword(token) => TokenInfo(token, TokenType.Keyword),
                    _ when IsStringLiteral(token) => TokenInfo(token, TokenType.StringLiteral),
                    _ when IsNumericLiteral(token) => TokenInfo(token, TokenType.NumericLiteral),
                    _ when IsIdentifier(token) && !skipIdentifiers => ClassifyIdentifier(token),
                    _ => null,
                };

            protected static TokenInfo TokenInfo(SyntaxToken token, TokenType tokenType) =>
                tokenType == TokenType.UnknownTokentype
                || (string.IsNullOrWhiteSpace(token.Text) && tokenType != TokenType.StringLiteral)
                    ? null
                    : new()
                    {
                        TokenType = tokenType,
                        TextRange = GetTextRange(token.GetLocation().GetLineSpan()),
                    };

            protected virtual TokenInfo ClassifyIdentifier(SyntaxToken token)
            {
                if (SemanticModel.GetDeclaredSymbol(token.Parent) is { } declaration)
                {
                    return ClassifyIdentifier(token, declaration);
                }
                else if (GetBindableParent(token) is { } parent && SemanticModel.GetSymbolInfo(parent).Symbol is { } symbol)
                {
                    return ClassifyIdentifier(token, symbol);
                }
                else
                {
                    return null;
                }
            }

            private TokenInfo ClassifyIdentifier(SyntaxToken token, ISymbol symbol) =>
                symbol switch
                {
                    IAliasSymbol alias => ClassifyIdentifier(token, alias.Target),
                    IMethodSymbol ctorSymbol when ConstructorKinds.Contains(ctorSymbol.MethodKind) => TokenInfo(token, TokenType.TypeName),
                    _ when token.ValueText == "var" && VarSymbolKinds.Contains(symbol.Kind) => TokenInfo(token, TokenType.Keyword),
                    { Kind: SymbolKind.Parameter, IsImplicitlyDeclared: true } when token.ValueText == "value" => TokenInfo(token, TokenType.Keyword),
                    { Kind: SymbolKind.NamedType or SymbolKind.TypeParameter } => TokenInfo(token, TokenType.TypeName),
                    { Kind: SymbolKind.DynamicType } => TokenInfo(token, TokenType.Keyword),
                    _ => null,
                };
        }

        protected internal abstract class TriviaClassifierBase
        {
            protected abstract bool IsDocComment(SyntaxTrivia trivia);
            protected abstract bool IsRegularComment(SyntaxTrivia trivia);

            public TokenInfo ClassifyTrivia(SyntaxTrivia trivia) =>
                trivia switch
                {
                    _ when IsRegularComment(trivia) => TokenInfo(trivia.SyntaxTree, TokenType.Comment, trivia.Span),
                    _ when IsDocComment(trivia) => ClassifyDocComment(trivia),
                    // Handle preprocessor directives here
                    _ => null,
                };

            private TokenInfo TokenInfo(SyntaxTree tree, TokenType tokenType, TextSpan span) =>
                new()
                {
                    TokenType = tokenType,
                    TextRange = GetTextRange(Location.Create(tree, span).GetLineSpan())
                };

            private TokenInfo ClassifyDocComment(SyntaxTrivia trivia) =>
                TokenInfo(trivia.SyntaxTree, TokenType.Comment, trivia.FullSpan);
        }
    }
}
