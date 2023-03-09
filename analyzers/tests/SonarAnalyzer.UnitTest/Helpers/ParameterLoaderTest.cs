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

using System.IO;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class ParameterLoaderTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataRow("path//aSonarLint.xml")] // different name
        [DataRow("path//SonarLint.xmla")] // different extension
        public void SetParameterValues_WhenNoSonarLintIsGiven_DoesNotPopulateParameters(string filePath)
        {
            // Arrange
            var sut = CreateSutWithOption(filePath, SourceText.From(File.ReadAllText("ResourceTests\\SonarLint.xml")));
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());

            // Assert
            analyzer.Maximum.Should().Be(3); // Default value
        }

        [TestMethod]
        [DataRow("a/SonarLint.xml")] // unix path
        [DataRow("a\\SonarLint.xml")]
        public void SetParameterValues_WhenGivenValidSonarLintFilePath_PopulatesProperties(string filePath)
        {
            // Arrange
            var sut = CreateSutWithOption(filePath, SourceText.From(File.ReadAllText("ResourceTests\\SonarLint.xml")));
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());

            // Assert
            analyzer.Maximum.Should().Be(1); // Value from the xml file
        }

        [TestMethod]
        public void SetParameterValues_WhenGivenSonarLintFileHasIntParameterType_PopulatesProperties()
        {
            // Arrange
            var sut = CreateSutWithOption("ResourceTests\\SonarLint.xml");
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());

            // Assert
            analyzer.Maximum.Should().Be(1); // Value from the xml file
        }

        [TestMethod]
        public void SetParameterValues_WhenGivenSonarLintFileHasStringParameterType_OnlyOneParameter_PopulatesProperty()
        {
            // Arrange
            var sut = CreateSutWithOption("ResourceTests\\RuleWithStringParameter\\SonarLint.xml");
            var analyzer = new EnumNameShouldFollowRegex(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());

            // Assert
            analyzer.FlagsEnumNamePattern.Should().Be("1"); // value from XML file
        }

        [TestMethod]
        public void SetParameterValues_WhenGivenSonarLintFileHasBooleanParameterType_OnlyOneParameter_PopulatesProperty()
        {
            // Arrange
            var sut = CreateSutWithOption("ResourceTests\\RuleWithBooleanParameter\\SonarLint.xml");
            var analyzer = new CheckFileLicense(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());

            // Assert
            analyzer.IsRegularExpression.Should().BeTrue(); // value from XML file
        }

        [TestMethod]
        public void SetParameterValues_WhenGivenValidSonarLintFileAndDoesNotContainAnalyzerParameters_DoesNotPopulateProperties()
        {
            // Arrange
            var sut = CreateSutWithOption("ResourceTests\\SonarLint.xml");
            var analyzer = new LineLength(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());

            // Assert
            analyzer.Maximum.Should().Be(200); // Default value
        }

        [TestMethod]
        public void SetParameterValues_WithNonExistentPath_UsesInMemoryText()
        {
            // Arrange
            var maxValue = 1;
            var ruleParameters = new List<SonarLintXmlRule>()
            {
                new SonarLintXmlRule()
                {
                    Key = "S1067",
                    Parameters = new List<SonarLintXmlKeyValuePair>()
                    {
                        new SonarLintXmlKeyValuePair()
                        {
                            Key = "max",
                            Value = maxValue.ToString()
                        }
                    }
                }
            };
            var sonarLintXml = AnalysisScaffolding.CreateSonarLintXml(TestContext, rulesParameters: ruleParameters);
            var sut = CreateSutWithOption(sonarLintXml);
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());

            // Assert
            analyzer.Maximum.Should().Be(maxValue); // In-memory value
        }

        [TestMethod]
        public void SetParameterValues_CalledTwiceAfterChangeInConfigFile_UpdatesProperties()
        {
            // Arrange
            var maxValue = 1;
            var ruleParameters = new List<SonarLintXmlRule>()
            {
                new SonarLintXmlRule()
                {
                    Key = "S1067",
                    Parameters = new List<SonarLintXmlKeyValuePair>()
                    {
                        new SonarLintXmlKeyValuePair()
                        {
                            Key = "max",
                            Value = maxValue.ToString()
                        }
                    }
                }
            };
            var sonarLintXml = AnalysisScaffolding.GenerateSonarLintXmlContent(rulesParameters: ruleParameters);
            var filePath = TestHelper.WriteFile(TestContext, "SonarLint.xml", sonarLintXml);
            var sut = CreateSutWithOption(filePath);
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());
            analyzer.Maximum.Should().Be(maxValue);

            // Modify the in-memory additional file
            maxValue = 42;
            ruleParameters.First().Parameters.First().Value = maxValue.ToString();
            var modifiedSonarLintXml = AnalysisScaffolding.GenerateSonarLintXmlContent(rulesParameters: ruleParameters);
            var modifiedFilePath = TestHelper.WriteFile(TestContext, "SonarLint.xml", modifiedSonarLintXml);
            sut = CreateSutWithOption(modifiedFilePath);

            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());
            analyzer.Maximum.Should().Be(maxValue);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("this is not an xml")]
        [DataRow(@"<?xml version=""1.0"" encoding=""UTF - 8""?><AnalysisInput><Settings>")]
        public void SetParameterValues_WithMalformedXml_DoesNotPopulateProperties(string sonarLintXmlContent)
        {
            // Arrange
            var sut = CreateSutWithOption("fakePath\\SonarLint.xml", SourceText.From(sonarLintXmlContent));
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());

            // Assert
            analyzer.Maximum.Should().Be(3); // Default value
        }

        [TestMethod]
        public void SetParameterValues_WithWrongPropertyType_StringInsteadOfInt_DoesNotPopulateProperties()
        {
            // Arrange
            var sut = CreateSutWithOption("ResourceTests\\StringInsteadOfInt\\SonarLint.xml");
            var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());

            // Assert
            analyzer.Maximum.Should().Be(3); // Default value
        }

        [TestMethod]
        public void SetParameterValues_WithWrongPropertyType_StringInsteadOfBoolean_DoesNotPopulateProperties()
        {
            // Arrange
            var sut = CreateSutWithOption("ResourceTests\\StringInsteadOfBoolean\\SonarLint.xml");
            var analyzer = new CheckFileLicense(); // Cannot use mock because we use reflection to find properties.

            // Act
            ParameterLoader.SetParameterValues(analyzer, sut.SonarLintFile());

            // Assert
            analyzer.IsRegularExpression.Should().BeFalse(); // Default value
        }

        private static SonarCompilationReportingContext CreateSutWithOption(string filePath, SourceText text = null)
        {
            var options = text is null
                ? AnalysisScaffolding.CreateOptions(filePath)
                : AnalysisScaffolding.CreateOptions(filePath, text);
            var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation();
            return CreateSut(compilation, options);
        }

        private static SonarCompilationReportingContext CreateSut(Compilation compilation, AnalyzerOptions options)
        {
            var compilationContext = new CompilationAnalysisContext(compilation, options, _ => { }, _ => true, default);
            return new(AnalysisScaffolding.CreateSonarAnalysisContext(), compilationContext);
        }
    }
}
