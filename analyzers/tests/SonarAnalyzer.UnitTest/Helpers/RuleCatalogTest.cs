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

using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
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
        public void Category_IsGenerated(string id, string expected)
        {
            RuleCatalogCS.Rules[id].Category.Should().Be(expected);
            RuleCatalogVB.Rules[id].Category.Should().Be(expected);
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
                "<p>So the use of the tabulation character must be banned.</p>",    // Asserting existence of the second paragraph that should not be part of the description
                "Developers should not need to configure the tab width of their text editors in order to be able to read source code.");

        [TestMethod]
        public void Description_TagsAreRemoved() =>
            ValidateDescription("S1116", "i.e. <code>;</code>, are", "Empty statements, i.e. ;, are usually introduced by mistake, for example because:");

        [TestMethod]
        public void Description_HtmlIsDecoded() =>
            ValidateDescription("S1067", "<code>&amp;&amp;</code>", "The complexity of an expression is defined by the number of &&, || and condition ? ifTrue : ifFalse operators it contains.");

        [TestMethod]
        public void Description_NewLinesAreSpaces() =>
            ValidateDescription(
                "S107",
                "too many\nthings.",    // Html contains new line with no spaces around it
                "A long parameter list can indicate that a new structure should be created to wrap the numerous parameters or that the function is doing too many things.");

        private static void AssertRuleS103(RuleDescriptor rule)
        {
            rule.Id.Should().Be("S103");
            rule.Title.Should().Be("Lines should not be too long");
            rule.Category.Should().Be("CODE_SMELL");
            rule.DefaultSeverity.Should().Be("Major");
            rule.Scope.Should().Be(SourceScope.All);
            rule.SonarWay.Should().BeFalse();
            rule.Description.Should().Be("Having to scroll horizontally makes it harder to get a quick overview and understanding of any piece of code.");
        }

        private static void ValidateDescription(string id, string assertedSourceFragment, string expectedDescription)
        {
            var rspecDirectory = Path.GetFullPath(Path.Combine(typeof(RuleCatalogTest).Assembly.Location, "..", "..", "..", "..", "..", "..", "rspec", "cs"));   // analyzers/rspec/cs
            var html = File.ReadAllText(Path.Combine(rspecDirectory, $"{id}_c#.html"));
            html.Should().Contain(assertedSourceFragment, "we need to make sure that the assertion below has expected data fragment as an input");
            RuleCatalogCS.Rules[id].Description.Should().Contain(expectedDescription);
        }
    }
}
