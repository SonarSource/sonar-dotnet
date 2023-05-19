using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;
using Moq;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules.CSharp;
using Match = System.Text.RegularExpressions.Match;

namespace SonarAnalyzer.UnitTest.Rules;

public partial class TokenTypeAnalyzerTest
{
    private static class ClassifierTestHarness
    {
        private const int TokenAnnotationChars = 4; // [u:]
        private const int PrefixTokenAnnotationChars = 3; // [u:

        public static void AssertTokenTypes(string code, bool allowSemanticModel = true, bool ignoreCompilationErrors = false)
        {
            var (tree, model, expectedTokens) = ParseTokens(code, ignoreCompilationErrors);
            var root = tree.GetRoot();
            model = allowSemanticModel ? model : new Mock<SemanticModel>(MockBehavior.Strict).Object; // The Mock will throw if the semantic model was used.
            var tokenClassifier = new SonarAnalyzer.Rules.CSharp.TokenTypeAnalyzer.TokenClassifier(model, false);
            var triviaClassifier = new SonarAnalyzer.Rules.CSharp.TokenTypeAnalyzer.TriviaClassifier();
            expectedTokens.Should().SatisfyRespectively(expectedTokens.Select((Func<ExpectedToken, Action<ExpectedToken>>)(e => CheckClassifiedToken)));

            void CheckClassifiedToken(ExpectedToken expected)
            {
                var expectedLineSpan = tree.GetLocation(expected.Position).GetLineSpan();
                var because = $$"""token with text "{{expected.TokenText}}" at position {{expectedLineSpan}} was marked as {{expected.TokenType}}""";
                var (actualLocation, classification) = FindActual();
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

                (Location, TokenTypeInfo.Types.TokenInfo TokenInfo) FindActual()
                {
                    if (expected.TokenType == TokenType.Comment)
                    {
                        var trivia = root.FindTrivia(expected.Position.Start);
                        return (tree.GetLocation(trivia.FullSpan), triviaClassifier.ClassifyTrivia(trivia));
                    }
                    else
                    {
                        var token = root.FindToken(expected.Position.Start);
                        var f = () => tokenClassifier.ClassifyToken(token);
                        var tokenInfo = f.Should().NotThrow($"semanticModel should not be queried for {because}").Which;
                        return (token.GetLocation(), tokenInfo);
                    }
                }
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
            var (tree, model) = ignoreCompilationErrors ? TestHelper.CompileIgnoreErrorsCS(sb.ToString()) : TestHelper.CompileCS(sb.ToString());
            return (tree, model, expectedTokens);
        }

        private static string TokenGroups(params string[] groups) =>
            string.Join("|", groups);

        private static string TokenGroup(TokenType tokenType, string shortName) =>
            $$"""(?'{{tokenType}}'\[{{shortName}}\:[^\]]+\])""";

        private static readonly Regex TokenTypeRegEx = new(TokenGroups(
            TokenGroup(TokenType.Keyword, "k"),
            TokenGroup(TokenType.NumericLiteral, "n"),
            TokenGroup(TokenType.StringLiteral, "s"),
            TokenGroup(TokenType.TypeName, "t"),
            TokenGroup(TokenType.Comment, "c"),
            TokenGroup(TokenType.UnknownTokentype, "u")));

        private readonly record struct ExpectedToken(TokenType TokenType, string TokenText, TextSpan Position);
    }
}
