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
using SonarAnalyzer.Protobuf;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using System.Linq;

namespace SonarAnalyzer.Rules
{
    public abstract class TokenTypeAnalyzerBase : UtilityAnalyzerBase<TokenTypeInfo>
    {
        protected const string DiagnosticId = "S9999-token-type";
        protected const string Title = "Token type calculator";

        private static readonly DiagnosticDescriptor rule =
            new DiagnosticDescriptor(DiagnosticId, Title, string.Empty, string.Empty, DiagnosticSeverity.Warning,
                true, customTags: WellKnownDiagnosticTags.NotConfigurable);

        protected override DiagnosticDescriptor Rule => rule;

        internal const string TokenTypeFileName = "token-type.pb";

        protected sealed override string FileName => TokenTypeFileName;

        protected sealed override TokenTypeInfo GetMessage(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            return GetTokenTypeInfo(syntaxTree, semanticModel);
        }

        internal /* only for msbuild12 support */ TokenTypeInfo GetTokenTypeInfo(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var tokens = syntaxTree.GetRoot().DescendantTokens();
            var spans = new List<TokenTypeInfo.Types.TokenInfo>();
            foreach (var token in tokens)
            {
                spans.AddRange(GetTokenClassifier(token, semanticModel).Spans);
            }

            var tokenTypeInfo = new TokenTypeInfo
            {
                FilePath = syntaxTree.FilePath
            };
            tokenTypeInfo.TokenInfo.AddRange(spans.OrderBy(s => s.TextRange.StartLine).ThenBy(s => s.TextRange.StartOffset));
            return tokenTypeInfo;
        }

        protected abstract TokenClassifierBase GetTokenClassifier(SyntaxToken token, SemanticModel semanticModel);

        protected abstract class TokenClassifierBase
        {
            private readonly SyntaxToken token;
            private readonly SemanticModel semanticModel;
            private readonly List<TokenTypeInfo.Types.TokenInfo> spans = new List<TokenTypeInfo.Types.TokenInfo>();

            protected TokenClassifierBase(SyntaxToken token, SemanticModel semanticModel)
            {
                this.token = token;
                this.semanticModel = semanticModel;
            }

            public IEnumerable<TokenTypeInfo.Types.TokenInfo> Spans
            {
                get
                {
                    spans.Clear();
                    ClassifyToken();

                    foreach (var trivia in token.LeadingTrivia)
                    {
                        ClassifyTrivia(trivia);
                    }

                    foreach (var trivia in token.TrailingTrivia)
                    {
                        ClassifyTrivia(trivia);
                    }

                    return spans;
                }
            }

            private void ClassifyToken()
            {
                if (IsKeyword(token))
                {
                    CollectClassified(TokenType.Keyword, token.Span);
                    return;
                }

                if (IsStringLiteral(token))
                {
                    CollectClassified(TokenType.StringLiteral, token.Span);
                    return;
                }

                if (IsNumericLiteral(token))
                {
                    CollectClassified(TokenType.NumericLiteral, token.Span);
                    return;
                }

                if (IsIdentifier(token))
                {
                    ClassifyIdentifier(token, semanticModel);
                    return;
                }
            }

            private void ClassifyIdentifier(SyntaxToken token, SemanticModel semanticModel)
            {
                var declaration = semanticModel.GetDeclaredSymbol(token.Parent);
                if (declaration != null)
                {
                    ClassifyIdentifier(token, declaration);
                    return;
                }

                var parent = GetBindableParent(token);
                if (parent != null)
                {
                    var symbol = semanticModel.GetSymbolInfo(parent).Symbol;
                    if (symbol != null)
                    {
                        ClassifyIdentifier(token, symbol);
                        return;
                    }
                }

                if (IsContextualKeyword(token))
                {
                    CollectClassified(TokenType.Keyword, token.Span);
                }
            }

            protected abstract SyntaxNode GetBindableParent(SyntaxToken token);

            private static readonly ISet<MethodKind> ConstructorKinds = ImmutableHashSet.Create(
                MethodKind.Constructor,
                MethodKind.StaticConstructor,
                MethodKind.SharedConstructor);

            private void ClassifyIdentifier(SyntaxToken token, ISymbol symbol)
            {
                if (symbol.Kind == SymbolKind.Alias)
                {
                    ClassifyIdentifier(token, ((IAliasSymbol)symbol).Target);
                    return;
                }

                var ctorSymbol = symbol as IMethodSymbol;
                if (ctorSymbol != null && ConstructorKinds.Contains(ctorSymbol.MethodKind))
                {
                    CollectClassified(TokenType.TypeName, token.Span);
                    return;
                }

                if (token.ToString() == "var" &&
                    VarSymbolKinds.Contains(symbol.Kind))
                {
                    CollectClassified(TokenType.Keyword, token.Span);
                    return;
                }

                if (token.ToString() == "value" &&
                    symbol.Kind == SymbolKind.Parameter &&
                    symbol.IsImplicitlyDeclared)
                {
                    CollectClassified(TokenType.Keyword, token.Span);
                    return;
                }

                if (symbol.Kind == SymbolKind.NamedType ||
                    symbol.Kind == SymbolKind.TypeParameter)
                {
                    CollectClassified(TokenType.TypeName, token.Span);
                    return;
                }

                if (symbol.Kind == SymbolKind.DynamicType)
                {
                    CollectClassified(TokenType.Keyword, token.Span);
                }
            }

            private static readonly ISet<SymbolKind> VarSymbolKinds = ImmutableHashSet.Create(
                SymbolKind.NamedType,
                SymbolKind.TypeParameter,
                SymbolKind.ArrayType,
                SymbolKind.PointerType);

            private void ClassifyTrivia(SyntaxTrivia trivia)
            {
                if (IsRegularComment(trivia))
                {
                    CollectClassified(TokenType.Comment, trivia.Span);
                    return;
                }

                if (IsDocComment(trivia))
                {
                    ClassifyDocComment(trivia);
                    return;
                }

                // todo preprocessor directives
            }

            protected abstract bool IsDocComment(SyntaxTrivia trivia);

            private void ClassifyDocComment(SyntaxTrivia trivia)
            {
                CollectClassified(TokenType.Comment, trivia.FullSpan);
            }

            protected void CollectClassified(TokenType tokenType, TextSpan span)
            {
                if (string.IsNullOrWhiteSpace(token.SyntaxTree.GetText().GetSubText(span).ToString()))
                {
                    return;
                }

                spans.Add(new TokenTypeInfo.Types.TokenInfo
                {
                    TokenType = tokenType,
                    TextRange = GetTextRange(Location.Create(token.SyntaxTree, span).GetLineSpan())
                });
            }

            protected abstract bool IsRegularComment(SyntaxTrivia trivia);

            protected abstract bool IsKeyword(SyntaxToken token);

            protected abstract bool IsContextualKeyword(SyntaxToken token);

            protected abstract bool IsIdentifier(SyntaxToken token);

            protected abstract bool IsNumericLiteral(SyntaxToken token);

            protected abstract bool IsStringLiteral(SyntaxToken token);
        }
    }
}
