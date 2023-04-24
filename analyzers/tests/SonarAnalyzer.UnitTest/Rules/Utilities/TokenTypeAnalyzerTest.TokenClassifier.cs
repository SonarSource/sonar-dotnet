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

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.UnitTest.Rules;

public partial class TokenTypeAnalyzerTest
{
    private static Regex tokenTypeRegEx = new(
        """(?'Keyword'\[k\:[^\]]+\])|(?'NumericLiteral'\[n\:[^\]]+\])|(?'StringLiteral'\[s\:[^\]]+\])|(?'TypeName'\[t\:[^\]]+\])|(?'Comment'\[c\:[^\]]+\])|(?'UnknownTokentype'\[u\:[^\]]+\])""",
        RegexOptions.Compiled);

    [TestMethod]
    public void ClassClassifications()
        => AssertTokenTypes("""
            [k:using] [u:System];
            [k:public] [k:class] [t:Test]
            {
                [k:public] [t:Test]() { }

                [k:int] [u:Prop] { [k:get]; }
                [k:void] [u:Method]<[t:T]>([t:T] [u:t]) [k:where] [t:T]: [k:class], [t:IComparable], [u:System].[u:Collections].[t:IComparer]
                {
                    [k:var] [u:i] = [n:1];
                    var s = [s:"Hello"];
                    [t:T] [u:local] = [k:default];
                }
            [u:}]
            """);

    private static void AssertTokenTypes(string code)
    {
        var (tree, model, expected) = ParseTokens(code);
        var root = tree.GetRoot();
        var classifier = new SonarAnalyzer.Rules.CSharp.TokenTypeAnalyzer.TokenClassifier(model, false);
        foreach (var e in expected)
        {
            var because = $$"""expected token {{e.TokenType}} with text {{e.TokenText}} at position {{e.Postion}}""";
            var treeToken = root.FindToken(e.Postion.Start);
            var actual = classifier.ClassifyToken(treeToken);
            if (actual == null)
            {
                e.TokenType.Should().Be(TokenType.UnknownTokentype, because);
                treeToken.GetLocation().SourceSpan.Should().Be(e.Postion, because);
            }
            else
            {
                actual.TokenType.Should().Be(e.TokenType, because);
                var treeTokenRange = treeToken.GetLocation().GetLineSpan();
                actual.TextRange.Should().Be(new TextRange
                {
                    StartLine = treeTokenRange.StartLinePosition.Line + 1,
                    StartOffset = treeTokenRange.StartLinePosition.Character,
                    EndLine = treeTokenRange.EndLinePosition.Line + 1,
                    EndOffset = treeTokenRange.EndLinePosition.Character,
                }, because);
            }
        }

    }

    private static (SyntaxTree Tree, SemanticModel Model, IReadOnlyCollection<ExpectedToken> ExpectedTokens) ParseTokens(string code)
    {
        var matches = tokenTypeRegEx.Matches(code);
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

    private readonly record struct ExpectedToken(TokenType TokenType, string TokenText,  TextSpan Postion);
}
