extern alias csharp;
/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2019 SonarSource SA
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class PropertiesHelperTest
    {
        [TestMethod]
        public void ShouldAnalyzeGeneratedCode_WithTrueSetting_ReturnsTrue()
        {
            var result = GetSetting("ResourceTests\\AnalyzeGeneratedTrue\\SonarLint.xml");
            result.Should().BeTrue();
        }

        [TestMethod]
        public void ShouldAnalyzeGeneratedCode_WithFalseSetting_ReturnsFalse()
        {
            var result = GetSetting("ResourceTests\\AnalyzeGeneratedFalse\\SonarLint.xml");
            result.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldAnalyzeGeneratedCode_WithNoSetting_ReturnsFalse()
        {
            var result = GetSetting("ResourceTests\\NoSettings\\SonarLint.xml");
            result.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldAnalyzeGeneratedCode_WithMalformedXml_ReturnsFalse()
        {
            var result = GetSetting("ResourceTests\\Malformed\\SonarLint.xml");
            result.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldAnalyzeGeneratedCode_WithNotBooleanValue_ReturnsFalse()
        {
            var result = GetSetting("ResourceTests\\NotBoolean\\SonarLint.xml");
            result.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldAnalyzeGeneratedCode_WithNotExistingPath_ReturnsFalse()
        {
            GetSetting("ResourceTests\\NotExistingFolder\\SonarLint.xml");
        }

        private static bool GetSetting(string filePath)
        {
            // Arrange
            var additionalText = new Mock<AdditionalText>();
            additionalText.Setup(x => x.Path).Returns(filePath);
            var options = new AnalyzerOptions(ImmutableArray.Create(additionalText.Object));

            // Act
            return PropertiesHelper.ShouldAnalyzeGeneratedCode(options, LanguageNames.CSharp);
        }
    }
}
