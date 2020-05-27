/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

extern alias csharp;
using System.Collections.Immutable;
using System.IO;
using csharp::SonarAnalyzer.Rules.CSharp;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class ParameterLoaderTests
    {
        [TestMethod]
        public void SetParameterValues_WhenNoSonarLintIsGiven_DoesNotPopulateParameters()
        {
            // Arrange
            var options = CreateOptionsWithAdditionalFile("ResourceTests\\MyFile.xml");
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(3); // Default value
        }

        [TestMethod]
        public void SetParameterValues_WhenGivenSonarLintFileHasIntParameterType_PopulatesProperties()
        {
            // Arrange
            var options = CreateOptionsWithAdditionalFile("ResourceTests\\SonarLint.xml");
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(1); // Value from the xml file
        }

        [TestMethod]
        public void SetParameterValues_WhenGivenSonarLintFileHasStringParameterType_OnlyOneParameter_PopulatesProperty()
        {
            // Arrange
            var options = CreateOptionsWithAdditionalFile("ResourceTests\\RuleWithStringParameter\\SonarLint.xml");
            var analyzer = new EnumNameShouldFollowRegex(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.FlagsEnumNamePattern.Should().Be("1"); // value from XML file
        }

        [TestMethod]
        public void SetParameterValues_WhenGivenValidSonarLintFileAndDoesNotContainAnalyzerParameters_DoesNotPopulateProperties()
        {
            // Arrange
            var options = CreateOptionsWithAdditionalFile("ResourceTests\\SonarLint.xml");
            var analyzer = new LineLength(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(200); // Default value
        }

        [TestMethod]
        public void SetParameterValues_CalledTwiceAfterChangeInConfigFile_UpdatesProperties()
        {
            // Arrange
            var sonarLintXmlRelativePath = "ResourceTests\\ToChange\\SonarLint.xml";
            var options = CreateOptionsWithAdditionalFile(sonarLintXmlRelativePath);
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act with the file on disk
            ParameterLoader.SetParameterValues(analyzer, options);
            analyzer.Maximum.Should().Be(1); // from the XML on disk

            // Update the file on disk
            var originalContent = File.ReadAllText(sonarLintXmlRelativePath);
            var modifiedContent = originalContent.Replace("<Value>1</Value>", "<Value>42</Value>");
            File.WriteAllText(sonarLintXmlRelativePath, modifiedContent);
            ParameterLoader.SetParameterValues(analyzer, options);
            analyzer.Maximum.Should().Be(42);

            // revert change
            File.WriteAllText(sonarLintXmlRelativePath, originalContent);
        }

        [TestMethod]
        public void SetParameterValues_WithMalformedXml_DoesNotPopulateProperties()
        {
            // Arrange
            var options = CreateOptionsWithAdditionalFile("ResourceTests\\Malformed\\SonarLint.xml");
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(3); // Default value
        }

        [TestMethod]
        public void SetParameterValues_WithBadPath_DoesNotPopulateProperties()
        {
            // Arrange
            var options = CreateOptionsWithAdditionalFile("ResourceTests\\ThisPathDoesNotExist\\SonarLint.xml");
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(3); // Default value
        }

        [TestMethod]
        public void SetParameterValues_WithWrongPropertyType_StringInsteadOfInt_DoesNotPopulateProperties()
        {
            // Arrange
            var options = CreateOptionsWithAdditionalFile("ResourceTests\\StringInsteadOfInt\\SonarLint.xml");
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(3); // Default value
        }

        private static AnalyzerOptions CreateOptionsWithAdditionalFile(string relativePath)
        {
            var additionalText = new Mock<AdditionalText>();
            additionalText.Setup(x => x.Path).Returns(relativePath);
            return new AnalyzerOptions(ImmutableArray.Create(additionalText.Object));
        }
    }
}
