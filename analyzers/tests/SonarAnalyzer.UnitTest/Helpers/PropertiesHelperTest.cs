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

using System.Collections.Immutable;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class PropertiesHelperTest
    {
        [TestMethod]
        [DataRow("a/SonarLint.xml")] // unix path
        [DataRow("a\\SonarLint.xml")]
        public void ShouldAnalyzeGeneratedCode_WithTrueSetting_ReturnsTrue(string filePath) =>
            GetSetting(SourceText.From(File.ReadAllText("ResourceTests\\AnalyzeGeneratedTrue\\SonarLint.xml")), filePath).Should().BeTrue();

        [TestMethod]
        public void ShouldAnalyzeGeneratedCode_WithFalseSetting_ReturnsFalse() =>
            GetSetting(SourceText.From(File.ReadAllText("ResourceTests\\AnalyzeGeneratedFalse\\SonarLint.xml"))).Should().BeFalse();

        [TestMethod]
        public void ShouldAnalyzeGeneratedCode_WithNoSetting_ReturnsFalse() =>
            GetSetting(SourceText.From(File.ReadAllText("ResourceTests\\NoSettings\\SonarLint.xml"))).Should().BeFalse();

        [TestMethod]
        [DataRow("")]
        [DataRow("this is not an xml")]
        [DataRow(@"<?xml version=""1.0"" encoding=""UTF - 8""?><AnalysisInput><Settings>")]
        public void ShouldAnalyzeGeneratedCode_WithMalformedXml_ReturnsFalse(string sonarLintXmlContent) =>
            GetSetting(SourceText.From(sonarLintXmlContent)).Should().BeFalse();

        [TestMethod]
        public void ShouldAnalyzeGeneratedCode_WithNotBooleanValue_ReturnsFalse() =>
            GetSetting(SourceText.From(File.ReadAllText("ResourceTests\\NotBoolean\\SonarLint.xml"))).Should().BeFalse();

        [TestMethod]
        [DataRow("path//aSonarLint.xml")] // different name
        [DataRow("path//SonarLint.xmla")] // different extension
        public void ShouldAnalyzeGeneratedCode_NonSonarLintXmlPath_ReturnsFalse(string filePath) =>
            GetSetting(SourceText.From(File.ReadAllText("ResourceTests\\AnalyzeGeneratedTrue\\SonarLint.xml")), filePath).Should().BeFalse();

        private static bool GetSetting(SourceText content, string filePath = "fakePath\\SonarLint.xml")
        {
            // Arrange
            var additionalText = new Mock<AdditionalText>();
            additionalText.Setup(x => x.Path).Returns(filePath); // use in-memory additional file
            additionalText.Setup(x => x.GetText(default)).Returns(content);

            var options = new AnalyzerOptions(ImmutableArray.Create(additionalText.Object));

            // Act
            return PropertiesHelper.ReadAnalyzeGeneratedCodeProperty(PropertiesHelper.GetSettings(options), LanguageNames.CSharp);
        }
    }
}
