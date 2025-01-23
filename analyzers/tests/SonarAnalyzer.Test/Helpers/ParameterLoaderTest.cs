/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.IO;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Helpers;

[TestClass]
public class ParameterLoaderTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("path//aSonarLint.xml")] // different name
    [DataRow("path//SonarLint.xmla")] // different extension
    public void SetParameterValues_WithInvalidSonarLintPath_DoesNotPopulateParameters(string filePath)
    {
        // Arrange
        var compilation = CreateCompilationWithOption(filePath, SourceText.From(File.ReadAllText(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml")));
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        // Act
        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());

        // Assert
        analyzer.Maximum.Should().Be(3); // Default value
    }

    [TestMethod]
    [DataRow("a/SonarLint.xml")] // unix path
    [DataRow(@"a\SonarLint.xml")]
    public void SetParameterValues_WithValidSonarLintPath_PopulatesProperties(string filePath)
    {
        // Arrange
        var compilation = CreateCompilationWithOption(filePath, SourceText.From(File.ReadAllText(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml")));
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        // Act
        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());

        // Assert
        analyzer.Maximum.Should().Be(1); // Value from the xml file
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithIntParameterType_PopulatesProperties()
    {
        // Arrange
        var compilation = CreateCompilationWithOption(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml");
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        // Act
        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());

        // Assert
        analyzer.Maximum.Should().Be(1); // Value from the xml file
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithStringParameterType_PopulatesProperty()
    {
        // Arrange
        var parameterValue = "1";
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S2342", "flagsAttributeFormat", parameterValue);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new EnumNameShouldFollowRegex(); // Cannot use mock because we use reflection to find properties.

        // Act
        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());

        // Assert
        analyzer.FlagsEnumNamePattern.Should().Be(parameterValue); // value from XML file
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithBooleanParameterType_PopulatesProperty()
    {
        // Arrange
        var parameterValue = true;
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S1451", "isRegularExpression", parameterValue.ToString());
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new CheckFileLicense(); // Cannot use mock because we use reflection to find properties.

        // Act
        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());

        // Assert
        analyzer.IsRegularExpression.Should().Be(parameterValue); // value from XML file
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithoutRuleParameters_DoesNotPopulateProperties()
    {
        // Arrange
        var compilation = CreateCompilationWithOption(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml");
        var analyzer = new LineLength(); // Cannot use mock because we use reflection to find properties.

        // Act
        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());

        // Assert
        analyzer.Maximum.Should().Be(200); // Default value
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
        var filePath = TestFiles.WriteFile(TestContext, "SonarLint.xml", sonarLintXml);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        // Act
        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.Maximum.Should().Be(maxValue);

        // Modify the in-memory additional file
        maxValue = 42;
        ruleParameters.First().Parameters.First().Value = maxValue.ToString();
        var modifiedSonarLintXml = AnalysisScaffolding.GenerateSonarLintXmlContent(rulesParameters: ruleParameters);
        var modifiedFilePath = TestFiles.WriteFile(TestContext, "SonarLint.xml", modifiedSonarLintXml);
        compilation = CreateCompilationWithOption(modifiedFilePath);

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.Maximum.Should().Be(maxValue);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("this is not an xml")]
    [DataRow(@"<?xml version=""1.0"" encoding=""UTF - 8""?><AnalysisInput><Settings>")]
    public void SetParameterValues_WithMalformedXml_DoesNotPopulateProperties(string sonarLintXmlContent)
    {
        // Arrange
        var compilation = CreateCompilationWithOption(@"fakePath\SonarLint.xml", SourceText.From(sonarLintXmlContent));
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        // Act
        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());

        // Assert
        analyzer.Maximum.Should().Be(3); // Default value
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithStringInsteadOfIntParameterType_PopulatesProperty()
    {
        // Arrange
        var parameterValue = "fooBar";
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S1067", "max", parameterValue);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        // Act
        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());

        // Assert
        analyzer.Maximum.Should().Be(3); // Default value
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithStringInsteadOfBooleanParameterType_PopulatesProperty()
    {
        // Arrange
        var parameterValue = "fooBar";
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S1451", "isRegularExpression", parameterValue);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new CheckFileLicense(); // Cannot use mock because we use reflection to find properties.

        // Act
        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());

        // Assert
        analyzer.IsRegularExpression.Should().BeFalse(); // Default value
    }

    private static SonarCompilationReportingContext CreateCompilationWithOption(string filePath, SourceText text = null)
    {
        var options = text is null
            ? AnalysisScaffolding.CreateOptions(filePath)
            : AnalysisScaffolding.CreateOptions(filePath, text);
        var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation();
        var compilationContext = new CompilationAnalysisContext(compilation, options, _ => { }, _ => true, default);
        return new(AnalysisScaffolding.CreateSonarAnalysisContext(), compilationContext);
    }

    private string GenerateSonarLintXmlWithParametrizedRule(string ruleId, string key, string value)
    {
        var ruleParameters = new List<SonarLintXmlRule>()
        {
            new SonarLintXmlRule()
            {
                Key = ruleId,
                Parameters = new List<SonarLintXmlKeyValuePair>()
                {
                    new SonarLintXmlKeyValuePair()
                    {
                        Key = key,
                        Value = value
                    }
                }
            }
        };
        return AnalysisScaffolding.CreateSonarLintXml(TestContext, rulesParameters: ruleParameters);
    }
}
