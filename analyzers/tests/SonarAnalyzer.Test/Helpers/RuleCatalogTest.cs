/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.IO;
using RuleCatalogCS = SonarAnalyzer.CSharp.Core.Rspec.RuleCatalog;
using RuleCatalogVB = SonarAnalyzer.VisualBasic.Core.Rspec.RuleCatalog;

namespace SonarAnalyzer.Test.Helpers;

[TestClass]
public class RuleCatalogTest
{
    [TestMethod]
    public void RuleCatalog_HasAllFieldsSet_CS() =>
        AssertRuleS103(RuleCatalogCS.Rules["S103"]);

    [TestMethod]
    public void RuleCatalog_HasAllFieldsSet_VB() =>
        AssertRuleS103(RuleCatalogVB.Rules["S103"]);

    [DataTestMethod]
    [DataRow("S103", "CODE_SMELL")]
    [DataRow("S1048", "BUG")]
    [DataRow("S1313", "SECURITY_HOTSPOT")]
    public void Type_IsGenerated(string id, string expected)
    {
        RuleCatalogCS.Rules[id].Type.Should().Be(expected);
        RuleCatalogVB.Rules[id].Type.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("S101", SourceScope.All)]
    [DataRow("S112", SourceScope.Main)]
    [DataRow("S3431", SourceScope.Tests)]
    public void SourceScope_IsGenerated(string id, SourceScope expected)
    {
        RuleCatalogCS.Rules[id].Scope.Should().Be(expected);
        RuleCatalogVB.Rules[id].Scope.Should().Be(expected);
    }

    [TestMethod]
    public void Description_TakesFirstParagraph() =>
        ValidateDescription(
            "S105",
            "<p>That is why using spaces is preferable.</p>",    // Asserting existence of the second paragraph that should not be part of the description
            "The tab width can differ from one development environment to another." +
            " Using tabs may require other developers to configure their environment (text editor, preferences, etc.) to read source code.");

    [TestMethod]
    public void Description_TagsAreRemoved() =>
        ValidateDescription("S1116", "by a semicolon <code>;</code>", "Empty statements represented by a semicolon ; are statements that do not perform any operation.");

    [TestMethod]
    public void Description_HtmlIsDecoded() =>
        ValidateDescription("S1067", "<code>&amp;&amp;</code>", "The complexity of an expression is defined by the number of &&, || and condition ? ifTrue : ifFalse operators it contains.");

    [TestMethod]
    public void Description_NewLinesAreSpaces() =>
        ValidateDescription(
            "S107",
            "track of their\nposition.",    // Html contains new line with no spaces around it
            "Methods with a long parameter list are difficult to use because maintainers must figure out the role of each parameter and keep track of their position.");

    private static void AssertRuleS103(RuleDescriptor rule)
    {
        rule.Id.Should().Be("S103");
        rule.Title.Should().Be("Lines should not be too long");
        rule.Type.Should().Be("CODE_SMELL");
        rule.DefaultSeverity.Should().Be("Major");
        rule.Status.Should().Be("ready");
        rule.Scope.Should().Be(SourceScope.All);
        rule.SonarWay.Should().BeFalse();
        rule.Description.Should().Be("Scrolling horizontally to see a full line of code lowers the code readability.");
    }

    private static void ValidateDescription(string id, string assertedSourceFragment, string expectedDescription)
    {
        var rspecDirectory = Path.Combine(Paths.AnalyzersRoot, "rspec", "cs");
        var html = File.ReadAllText(Path.Combine(rspecDirectory, $"{id}.html")).Replace("\r\n", "\n");
        html.Should().Contain(assertedSourceFragment, "we need to make sure that the assertion below has expected data fragment as an input");
        RuleCatalogCS.Rules[id].Description.Should().Contain(expectedDescription);
    }
}
