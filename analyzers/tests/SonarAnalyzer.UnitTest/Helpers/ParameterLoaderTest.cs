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

using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class ParameterLoaderTest
    {
        [TestMethod]
        public void SetParameterValues_WhenNoSonarLintIsGiven_DoesNotPopulateParameters()
        {
            // Arrange
            var options = TestHelper.CreateOptions("ResourceTests\\MyFile.xml");
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
            var options = TestHelper.CreateOptions("ResourceTests\\SonarLint.xml");
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
            var options = TestHelper.CreateOptions("ResourceTests\\RuleWithStringParameter\\SonarLint.xml");
            var analyzer = new EnumNameShouldFollowRegex(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.FlagsEnumNamePattern.Should().Be("1"); // value from XML file
        }

        [TestMethod]
        public void SetParameterValues_WhenGivenSonarLintFileHasBooleanParameterType_OnlyOneParameter_PopulatesProperty()
        {
            // Arrange
            var options = TestHelper.CreateOptions("ResourceTests\\RuleWithBooleanParameter\\SonarLint.xml");
            var analyzer = new CheckFileLicense(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.IsRegularExpression.Should().BeTrue(); // value from XML file
        }

        [TestMethod]
        public void SetParameterValues_WhenGivenValidSonarLintFileAndDoesNotContainAnalyzerParameters_DoesNotPopulateProperties()
        {
            // Arrange
            var options = TestHelper.CreateOptions("ResourceTests\\SonarLint.xml");
            var analyzer = new LineLength(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(200); // Default value
        }

        [TestMethod]
        public void SetParameterValues_WithNonExistingPath_UsesInMemoryText()
        {
            // Arrange
            var inMemoryText = SourceText.From(File.ReadAllText("ResourceTests\\ToChange\\SonarLint.xml"));
            var options = TestHelper.CreateOptions("ResourceTests\\ThisPathDoesNotExist\\SonarLint.xml", inMemoryText);
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(1); // In-memory value
        }

        [TestMethod]
        public void SetParameterValues_CalledTwiceAfterChangeInConfigFile_UpdatesProperties()
        {
            // Arrange
            var inMemoryText = SourceText.From(File.ReadAllText("ResourceTests\\ToChange\\SonarLint.xml"));
            var sonarLintXmlRelativePath = "ResourceTests\\ThisPathDoesNotExist\\SonarLint.xml";
            var options = TestHelper.CreateOptions(sonarLintXmlRelativePath, inMemoryText);
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);
            analyzer.Maximum.Should().Be(1); // In-memory value

            // Modify the in-memory additional file
            var modifiedSourceText = SourceText.From(inMemoryText.ToString().Replace("<Value>1</Value>", "<Value>42</Value>"));
            var modifiedOptions = TestHelper.CreateOptions("fake\\SonarLint.xml", modifiedSourceText);

            ParameterLoader.SetParameterValues(analyzer, modifiedOptions);
            analyzer.Maximum.Should().Be(42);
        }

        [TestMethod]
        public void SetParameterValues_WithMalformedXml_DoesNotPopulateProperties()
        {
            // Arrange
            var options = TestHelper.CreateOptions("ResourceTests\\Malformed\\SonarLint.xml");
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
            var options = TestHelper.CreateOptions("ResourceTests\\StringInsteadOfInt\\SonarLint.xml");
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(3); // Default value
        }

        [TestMethod]
        public void SetParameterValues_WithWrongPropertyType_StringInsteadOfBoolean_DoesNotPopulateProperties()
        {
            // Arrange
            var options = TestHelper.CreateOptions("ResourceTests\\StringInsteadOfBoolean\\SonarLint.xml");
            var analyzer = new CheckFileLicense(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.IsRegularExpression.Should().BeFalse(); // Default value
        }
    }
}
