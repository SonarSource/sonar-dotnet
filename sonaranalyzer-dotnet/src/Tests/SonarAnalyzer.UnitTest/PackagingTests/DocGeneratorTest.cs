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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.RuleDocGenerator;
using SonarAnalyzer.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using SonarAnalyzer.RuleDescriptors;
using FluentAssertions;

namespace SonarAnalyzer.UnitTest.PackagingTests
{
    [TestClass]
    public class DocGeneratorTest
    {
        private const string TestCategoryName = "DocGenerator";

        [TestMethod]
        [TestCategory(TestCategoryName)]
        public void CheckNumberOfCrossReferences()
        {
            // These are the deprecated rules which reference their successors
            var crossReferenceCount = GetNumberOfCrossReferences(AnalyzerLanguage.CSharp);
            crossReferenceCount.Should().Be(7);
            crossReferenceCount = GetNumberOfCrossReferences(AnalyzerLanguage.VisualBasic);
            crossReferenceCount.Should().Be(1);
        }

        [TestMethod]
        [TestCategory(TestCategoryName)]
        public void CheckNumberOfCrossLinks()
        {
            var crossReferenceCount = GetNumberOfCrossReferences(AnalyzerLanguage.VisualBasic) + GetNumberOfCrossReferences(AnalyzerLanguage.CSharp);
            var productVersion = FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion;
            var json = Program.GenerateRuleJson(productVersion);

            var commonSubUrl = RuleImplementationMeta.HelpLinkPattern.Replace("{1}", string.Empty);
            var crossLinkCount = NumberOfOccurrences(json, string.Format(commonSubUrl, productVersion));

            crossLinkCount.Should().Be(crossReferenceCount);
        }

        [TestMethod]
        [TestCategory(TestCategoryName)]
        public void ConvertCsharpLinks()
        {
            // Arrange
            var ruleDetail = new RuleDetail
            {
                Description = @"<p>Some description here...</p>
<h2>Noncompliant Code Example</h2>
<pre>
if (str == null &amp;&amp; str.length() == 0) {
  System.out.println(""String is empty"");
}
</pre>
<p>Some text here; use Rule {rule:csharpsquid:S2259} instead.</p>
<p>Some more text here; use rule {rule:csharpsquid:S2259} instead.</p>
<p>Other text here; use {rule:csharpsquid:S2259} instead.</p>",
            };

            // Act
            var result = RuleImplementationMeta.Convert(ruleDetail, "arbitrary-version", AnalyzerLanguage.CSharp);

            var expected = @"<p>Some description here...</p>
<h2>Noncompliant Code Example</h2>
<pre>
if (str == null &amp;&amp; str.length() == 0) {
  System.out.println(""String is empty"");
}
</pre>
<p>Some text here; use <a class=""rule-link"" href=""#version=arbitrary-version&ruleId=S2259"">Rule S2259</a> instead.</p>
<p>Some more text here; use <a class=""rule-link"" href=""#version=arbitrary-version&ruleId=S2259"">Rule S2259</a> instead.</p>
<p>Other text here; use <a class=""rule-link"" href=""#version=arbitrary-version&ruleId=S2259"">Rule S2259</a> instead.</p>";

            // Assert
            result.Description.Should().Be(expected);
        }

        [TestMethod]
        [TestCategory(TestCategoryName)]
        public void ConvertVbLinks()
        {
            // Arrange
            var ruleDetail = new RuleDetail
            {
                Description = @"<p>Some description here...</p>
<h2>Noncompliant Code Example</h2>
<pre>
if (str == null &amp;&amp; str.length() == 0) {
  System.out.println(""String is empty"");
}
</pre>
<p>Some text here; use Rule {rule:vbnet:S2259} instead.</p>
<p>Some more text here; use rule {rule:vbnet:S2259} instead.</p>
<p>Other text here; use {rule:vbnet:S2259} instead.</p>",
            };

            // Act
            var result = RuleImplementationMeta.Convert(ruleDetail, "arbitrary-version", AnalyzerLanguage.CSharp);

            var expected = @"<p>Some description here...</p>
<h2>Noncompliant Code Example</h2>
<pre>
if (str == null &amp;&amp; str.length() == 0) {
  System.out.println(""String is empty"");
}
</pre>
<p>Some text here; use <a class=""rule-link"" href=""#version=arbitrary-version&ruleId=S2259"">Rule S2259</a> instead.</p>
<p>Some more text here; use <a class=""rule-link"" href=""#version=arbitrary-version&ruleId=S2259"">Rule S2259</a> instead.</p>
<p>Other text here; use <a class=""rule-link"" href=""#version=arbitrary-version&ruleId=S2259"">Rule S2259</a> instead.</p>";

            // Assert
            result.Description.Should().Be(expected);
        }

        private static int GetNumberOfCrossReferences(AnalyzerLanguage language)
        {
            return RuleDetailBuilder.GetParameterlessRuleDetails(language)
                .Select(rule => rule.Description)
                .Select(description => Regex.Matches(description, RuleImplementationMeta.CrosslinkPattern).Count)
                .Sum();
        }

        private static int NumberOfOccurrences(string source, string substring)
        {
            var replaced = source.Replace(substring, string.Empty);
            return (source.Length - replaced.Length) / substring.Length;
        }
    }
}
