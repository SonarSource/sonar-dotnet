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
        [DataRow("path//aSonarLint.xml")] // different name
        [DataRow("path//SonarLint.xmla")] // different extension
        public void SetParameterValues_WhenNoSonarLintIsGiven_DoesNotPopulateParameters(string filePath)
        {
            // Arrange
            var options = TestHelper.CreateOptions(filePath, SourceText.From(File.ReadAllText("ResourceTests\\SonarLint.xml")));
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(3); // Default value
        }

        [TestMethod]
        [DataRow("a/SonarLint.xml")] // unix path
        [DataRow("a\\SonarLint.xml")]
        public void SetParameterValues_WhenGivenValidSonarLintFilePath_PopulatesProperties(string filePath)
        {
            // Arrange
            var options = TestHelper.CreateOptions(filePath, SourceText.From(File.ReadAllText("ResourceTests\\SonarLint.xml")));
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);

            // Assert
            analyzer.Maximum.Should().Be(1); // Value from the xml file
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
        public void SetParameterValues_WithNonExistentPath_UsesInMemoryText()
        {
            // Arrange
            const string fakeSonarLintXmlFilePath = "ThisPathDoesNotExist\\SonarLint.xml";
            const string sonarLintXmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AnalysisInput>
  <Settings />
  <Rules>
    <Rule>
      <Key>S1067</Key>
      <Parameters>
        <Parameter>
          <Key>max</Key>
          <Value>1</Value>
        </Parameter>
      </Parameters>
    </Rule>
  </Rules>
  <Files>
  </Files>
</AnalysisInput>";

            var options = TestHelper.CreateOptions(fakeSonarLintXmlFilePath, SourceText.From(sonarLintXmlContent));
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
            const string fakeSonarLintXmlFilePath = "ThisPathDoesNotExist\\SonarLint.xml";
            const string originalSonarLintXmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AnalysisInput>
  <Settings />
  <Rules>
    <Rule>
      <Key>S1067</Key>
      <Parameters>
        <Parameter>
          <Key>max</Key>
          <Value>1</Value>
        </Parameter>
      </Parameters>
    </Rule>
  </Rules>
  <Files>
  </Files>
</AnalysisInput>";

            var options = TestHelper.CreateOptions(fakeSonarLintXmlFilePath, SourceText.From(originalSonarLintXmlContent));
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, options);
            analyzer.Maximum.Should().Be(1);

            // Modify the in-memory additional file
            var modifiedSonarLintXmlContent = originalSonarLintXmlContent.Replace("<Value>1</Value>", "<Value>42</Value>");
            var modifiedOptions = TestHelper.CreateOptions(fakeSonarLintXmlFilePath, SourceText.From(modifiedSonarLintXmlContent));

            ParameterLoader.SetParameterValues(analyzer, modifiedOptions);
            analyzer.Maximum.Should().Be(42);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("this is not an xml")]
        [DataRow(@"<?xml version=""1.0"" encoding=""UTF - 8""?><AnalysisInput><Settings>")]
        public void SetParameterValues_WithMalformedXml_DoesNotPopulateProperties(string sonarLintXmlContent)
        {
            // Arrange
            var options = TestHelper.CreateOptions("fakePath\\SonarLint.xml", SourceText.From(sonarLintXmlContent));
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
