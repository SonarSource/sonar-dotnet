/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class TokenTypeAnalyzerBase : UtilityAnalyzerBase<TokenTypeInfo>
    {
        protected const string DiagnosticId = "S9999-token-type";
        private const string Title = "Token type calculator";
        private const string TokenTypeFileName = "token-type.pb";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetUtilityDescriptor(DiagnosticId, Title);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected abstract TokenClassifierBase GetTokenClassifier(SyntaxToken token, SemanticModel semanticModel);

        protected override bool SkipAnalysisForLargeFiles => true;

        protected sealed override string FileName => TokenTypeFileName;

        protected sealed override TokenTypeInfo CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel)
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

        protected abstract class TokenClassifierBase
        {
            private readonly SyntaxToken token;
            private readonly SemanticModel semanticModel;
            private readonly List<TokenTypeInfo.Types.TokenInfo> spans = new List<TokenTypeInfo.Types.TokenInfo>();
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
            protected abstract bool IsDocComment(SyntaxTrivia trivia);
            protected abstract bool IsRegularComment(SyntaxTrivia trivia);
            protected abstract bool IsKeyword(SyntaxToken token);
            protected abstract bool IsIdentifier(SyntaxToken token);
            protected abstract bool IsNumericLiteral(SyntaxToken token);
            protected abstract bool IsStringLiteral(SyntaxToken token);

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

            private void CollectClassified(TokenType tokenType, TextSpan span)
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

            private void ClassifyToken()
            {
                if (IsKeyword(token))
                {
                    CollectClassified(TokenType.Keyword, token.Span);
                }
                else if (IsStringLiteral(token))
                {
                    CollectClassified(TokenType.StringLiteral, token.Span);
                }
                else if (IsNumericLiteral(token))
                {
                    CollectClassified(TokenType.NumericLiteral, token.Span);
                }
                else if (IsIdentifier(token))
                {
                    ClassifyIdentifier();
                }
            }

            private void ClassifyIdentifier()
            {
                if (semanticModel.GetDeclaredSymbol(token.Parent) is { } declaration)
                {
                    ClassifyIdentifier(declaration);
                }
                else if (GetBindableParent(token) is { }  parent && semanticModel.GetSymbolInfo(parent).Symbol is { } symbol)
                {
                    ClassifyIdentifier(symbol);
                }
            }

            private void ClassifyIdentifier(ISymbol symbol)
            {
                if (symbol.Kind == SymbolKind.Alias)
                {
                    ClassifyIdentifier(((IAliasSymbol)symbol).Target);
                }
                else if (symbol is IMethodSymbol ctorSymbol && ConstructorKinds.Contains(ctorSymbol.MethodKind))
                {
                    CollectClassified(TokenType.TypeName, token.Span);
                }
                else if (token.ToString() == "var" && VarSymbolKinds.Contains(symbol.Kind))
                {
                    CollectClassified(TokenType.Keyword, token.Span);
                }
                else if (token.ToString() == "value" && symbol.Kind == SymbolKind.Parameter && symbol.IsImplicitlyDeclared)
                {
                    CollectClassified(TokenType.Keyword, token.Span);
                }
                else if (symbol.Kind == SymbolKind.NamedType || symbol.Kind == SymbolKind.TypeParameter)
                {
                    CollectClassified(TokenType.TypeName, token.Span);
                }
                else if (symbol.Kind == SymbolKind.DynamicType)
                {
                    CollectClassified(TokenType.Keyword, token.Span);
                }
            }

            private void ClassifyTrivia(SyntaxTrivia trivia)
            {
                if (IsRegularComment(trivia))
                {
                    CollectClassified(TokenType.Comment, trivia.Span);
                }
                else if (IsDocComment(trivia))
                {
                    ClassifyDocComment(trivia);
                }
                // Handle preprocessor directives here
            }

            private void ClassifyDocComment(SyntaxTrivia trivia) =>
                CollectClassified(TokenType.Comment, trivia.FullSpan);
        }
    }
}
