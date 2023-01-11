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

using SonarAnalyzer.Protobuf;
using static SonarAnalyzer.ShimLayer.ClassificationTypeNames;

namespace SonarAnalyzer.Rules
{
    public abstract class TokenTypeAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, TokenTypeInfo>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S9999-token-type";
        private const string Title = "Token type calculator";

        protected sealed override string FileName => "token-type.pb";

        protected TokenTypeAnalyzerBase() : base(DiagnosticId, Title) { }

        protected abstract string ClassifyToken(SyntaxToken token);
        protected abstract TokenType ClassifyTrivia(SyntaxTrivia trivia);
        protected abstract bool IsTypeIdentifier(SemanticModel semanticModel, SyntaxToken token);

        protected sealed override TokenTypeInfo CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var tokens = syntaxTree.GetRoot().DescendantTokens();

            var spans = new List<TokenTypeInfo.Types.TokenInfo>();
            foreach (var token in tokens)
            {
                var tokenClassification = ClassifyToken(token);
                var tokenType = tokenClassification switch
                {
                    Keyword
                        or ControlKeyword
                        or PreprocessorKeyword => TokenType.Keyword,
                    Comment
                        or RegexComment => TokenType.Comment,
                    NumericLiteral => TokenType.NumericLiteral,
                    StringLiteral => TokenType.StringLiteral,
                    ClassName
                        or RecordClassName
                        or DelegateName
                        or EnumName
                        or InterfaceName
                        or ModuleName
                        or StructName
                        or RecordStructName
                        or TypeParameterName => TokenType.TypeName,
                    Identifier => IsTypeIdentifier(semanticModel, token)
                        ? TokenType.TypeName
                        : TokenType.UnknownTokentype,
                    _ => TokenType.UnknownTokentype,
                };

                Append(tokenType, token.GetLocation());
                foreach (var trivia in token.LeadingTrivia.Concat(token.TrailingTrivia))
                {
                    var triviaType = ClassifyTrivia(trivia);
                    Append(triviaType, trivia.GetLocation());
                }
            }

            var tokenTypeInfo = new TokenTypeInfo
            {
                FilePath = syntaxTree.FilePath
            };

            tokenTypeInfo.TokenInfo.AddRange(spans.OrderBy(s => s.TextRange.StartLine).ThenBy(s => s.TextRange.StartOffset));
            return tokenTypeInfo;

            void Append(TokenType tokenType, Location location)
            {
                if (tokenType != TokenType.UnknownTokentype)
                {
                    spans.Add(new TokenTypeInfo.Types.TokenInfo
                    {
                        TokenType = tokenType,
                        TextRange = GetTextRange(location.GetLineSpan()),
                    });
                }
            }
        }
    }
}
