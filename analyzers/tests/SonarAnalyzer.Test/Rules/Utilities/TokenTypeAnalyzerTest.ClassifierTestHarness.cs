/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Protobuf;
using static SonarAnalyzer.Rules.CSharp.TokenTypeAnalyzer;
using Match = System.Text.RegularExpressions.Match;

namespace SonarAnalyzer.Test.Rules;

public partial class TokenTypeAnalyzerTest
{
    private static class ClassifierTestHarness
    {
        private const int TokenAnnotationChars = 4; // [u:]
        private const int PrefixTokenAnnotationChars = 3; // [u:
        private static readonly Regex TokenTypeRegEx = new(TokenGroups(
            TokenGroup(TokenType.Keyword, "k"),
            TokenGroup(TokenType.NumericLiteral, "n"),
            TokenGroup(TokenType.StringLiteral, "s"),
            TokenGroup(TokenType.TypeName, "t"),
            TokenGroup(TokenType.Comment, "c"),
            TokenGroup(TokenType.UnknownTokentype, "u")));

        public static void AssertTokenTypes(string code, bool allowSemanticModel = true, bool ignoreCompilationErrors = false)
        {
            var (tree, model, expectedTokens) = ParseTokens(code, ignoreCompilationErrors);
            model = allowSemanticModel ? model : null; // The TokenClassifier will throw if the semantic model is used.
            var tokenClassifier = new TokenClassifier(model, false);
            var triviaClassifier = new TriviaClassifier();
            expectedTokens.Should().SatisfyRespectively(expectedTokens.Select(
                (Func<ExpectedToken, Action<ExpectedToken>>)(_ => token => CheckClassifiedToken(tokenClassifier, triviaClassifier, tree, token))));
        }

        private static void CheckClassifiedToken(TokenClassifier tokenClassifier, TriviaClassifier triviaClassifier, SyntaxTree tree, ExpectedToken expected)
        {
            var expectedLineSpan = tree.GetLocation(expected.Position).GetLineSpan();
            var because = $$"""token with text "{{expected.TokenText}}" at position {{expectedLineSpan}} was marked as {{expected.TokenType}}""";
            var (actualLocation, classification) = FindActual(tokenClassifier, triviaClassifier, tree, expected, because);
            if (classification == null)
            {
                because = $$"""classification for token with text "{{expected.TokenText}}" at position {{expectedLineSpan}} is null""";
                expected.TokenType.Should().Be(TokenType.UnknownTokentype, because);
                actualLocation.SourceSpan.Should().Be(expected.Position, because);
            }
            else
            {
                classification.Should().Be(new TokenTypeInfo.Types.TokenInfo
                {
                    TokenType = expected.TokenType,
                    TextRange = new TextRange
                    {
                        StartLine = expectedLineSpan.StartLinePosition.Line + 1,
                        StartOffset = expectedLineSpan.StartLinePosition.Character,
                        EndLine = expectedLineSpan.EndLinePosition.Line + 1,
                        EndOffset = expectedLineSpan.EndLinePosition.Character,
                    },
                }, because);
            }
        }

        private static (Location Location, TokenTypeInfo.Types.TokenInfo TokenInfo) FindActual(TokenClassifier tokenClassifier, TriviaClassifier triviaClassifier, SyntaxTree tree, ExpectedToken expected, string because)
        {
            if (expected.TokenType == TokenType.Comment)
            {
                var trivia = tree.GetRoot().FindTrivia(expected.Position.Start);
                return (tree.GetLocation(trivia.FullSpan), triviaClassifier.ClassifyTrivia(trivia));
            }
            else
            {
                var token = tree.GetRoot().FindToken(expected.Position.Start);
                var f = () => tokenClassifier.ClassifyToken(token);
                var tokenInfo = f.Should().NotThrow($"semanticModel should not be queried for {because}").Which;
                return (token.GetLocation(), tokenInfo);
            }
        }

        private static (SyntaxTree Tree, SemanticModel Model, IReadOnlyCollection<ExpectedToken> ExpectedTokens) ParseTokens(string code, bool ignoreCompilationErrors = false)
        {
            var matches = TokenTypeRegEx.Matches(code);
            var sb = new StringBuilder(code.Length);
            var expectedTokens = new List<ExpectedToken>(matches.Count);
            var lastMatchEnd = 0;
            var match = 0;
            foreach (var group in matches.Cast<Match>().Select(m => m.Groups.Cast<Group>().First(g => g.Success && g.Name != "0")))
            {
                var expectedTokenType = (TokenType)Enum.Parse(typeof(TokenType), group.Name);
                var position = group.Index - (match * TokenAnnotationChars);
                var length = group.Length - TokenAnnotationChars;
                var tokenText = group.Value.Substring(PrefixTokenAnnotationChars, group.Value.Length - TokenAnnotationChars);
                expectedTokens.Add(new ExpectedToken(expectedTokenType, tokenText, new TextSpan(position, length)));

                sb.Append(code.Substring(lastMatchEnd, group.Index - lastMatchEnd));
                sb.Append(tokenText);
                lastMatchEnd = group.Index + group.Length;
                match++;
            }
            sb.Append(code.Substring(lastMatchEnd));
            var (tree, model) = ignoreCompilationErrors ? TestCompiler.CompileIgnoreErrorsCS(sb.ToString()) : TestCompiler.CompileCS(sb.ToString());
            return (tree, model, expectedTokens);
        }

        private static string TokenGroups(params string[] groups) =>
            string.Join("|", groups);

        private static string TokenGroup(TokenType tokenType, string shortName) =>
            $$"""(?'{{tokenType}}'\[{{shortName}}\:[^\]]+\])""";

        private readonly record struct ExpectedToken(TokenType TokenType, string TokenText, TextSpan Position);
    }
}
