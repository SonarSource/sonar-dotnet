﻿using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;
using Moq;
using SonarAnalyzer.Protobuf;
using Match = System.Text.RegularExpressions.Match;

namespace SonarAnalyzer.UnitTest.Rules;

public partial class TokenTypeAnalyzerTest
{
    private static class ClassifierTestHarness
    {
        private static readonly Regex TokenTypeRegEx = new(TokenGroups(
            TokenGroup(TokenType.Keyword, "k"),
            TokenGroup(TokenType.NumericLiteral, "n"),
            TokenGroup(TokenType.StringLiteral, "s"),
            TokenGroup(TokenType.TypeName, "t"),
            TokenGroup(TokenType.Comment, "c"),
            TokenGroup(TokenType.UnknownTokentype, "u")));

        public static void AssertTokenTypes(string code, bool allowSemanticModel = true)
        {
            var (tree, model, expectedTokens) = ParseTokens(code);
            var root = tree.GetRoot();
            model = allowSemanticModel ? model : new Mock<SemanticModel>(MockBehavior.Strict).Object;
            var tokenClassifier = new SonarAnalyzer.Rules.CSharp.TokenTypeAnalyzer.TokenClassifier(model, false);
            var triviaClassifier = new SonarAnalyzer.Rules.CSharp.TokenTypeAnalyzer.TriviaClassifier();
            expectedTokens.Should().SatisfyRespectively(expectedTokens.Select((Func<ExpectedToken, Action<ExpectedToken>>)(e => expected =>
            {
                var expectedLineSpan = tree.GetLocation(expected.Postion).GetLineSpan();
                var because = $$"""token with text "{{expected.TokenText}}" at position {{expectedLineSpan}} was marked as {{expected.TokenType}}""";
                var (actualLocation, classification) = FindActual();
                if (classification == null)
                {
                    because = $$"""classification for token with text "{{expected.TokenText}}" at position {{expectedLineSpan}} is null""";
                    expected.TokenType.Should().Be(TokenType.UnknownTokentype, because);
                    actualLocation.SourceSpan.Should().Be(expected.Postion, because);
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

                (Location, TokenTypeInfo.Types.TokenInfo TokenInfo) FindActual()
                {
                    if (expected.TokenType == TokenType.Comment)
                    {
                        var trivia = root.FindTrivia(expected.Postion.Start);
                        return (tree.GetLocation(trivia.FullSpan), triviaClassifier.ClassifyTrivia(trivia));
                    }
                    else
                    {
                        var token = root.FindToken(expected.Postion.Start);
                        var f = () => tokenClassifier.ClassifyToken(token);
                        var tokenInfo = f.Should().NotThrow($"semanticModel should not be queried for {because}").Which;
                        return (token.GetLocation(), tokenInfo);
                    }
                }
            })));
        }

        private static (SyntaxTree Tree, SemanticModel Model, IReadOnlyCollection<ExpectedToken> ExpectedTokens) ParseTokens(string code)
        {
            var matches = TokenTypeRegEx.Matches(code);
            var sb = new StringBuilder(code.Length);
            var expectedTokens = new List<ExpectedToken>(matches.Count);
            var lastMatchEnd = 0;
            var match = 0;
            foreach (var group in matches.Cast<Match>().Select(m => m.Groups.Cast<Group>().First(g => g.Success && g.Name != "0")))
            {
                var expectedTokenType = (TokenType)Enum.Parse(typeof(TokenType), group.Name);
                var position = group.Index - (match * 4);
                var length = group.Length - 4;
                var tokenText = group.Value.Substring(3, group.Value.Length - 4);
                expectedTokens.Add(new ExpectedToken(expectedTokenType, tokenText, new TextSpan(position, length)));

                sb.Append(code.Substring(lastMatchEnd, group.Index - lastMatchEnd));
                sb.Append(tokenText);
                lastMatchEnd = group.Index + group.Length;
                match++;
            }
            sb.Append(code.Substring(lastMatchEnd));
            var (tree, model) = TestHelper.CompileCS(sb.ToString());
            return (tree, model, expectedTokens);
        }

        private static string TokenGroups(params string[] groups)
            => string.Join("|", groups);

        private static string TokenGroup(TokenType tokenType, string shortName)
            => $$"""(?'{{tokenType}}'\[{{shortName}}\:[^\]]+\])""";

        private readonly record struct ExpectedToken(TokenType TokenType, string TokenText, TextSpan Postion);
    }
}