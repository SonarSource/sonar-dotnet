/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Common;

[TestClass]
public class ParameterLoaderTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("path//aSonarLint.xml")] // different name
    [DataRow("path//SonarLint.xmla")] // different extension
    public void SetParameterValues_WithInvalidSonarLintPath_DoesNotPopulateParameters(string filePath)
    {
        var compilation = CreateCompilationWithOption(filePath, SourceText.From(File.ReadAllText(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml")));
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.Maximum.Should().Be(3); // Default value
    }

    [TestMethod]
    [DataRow("a/SonarLint.xml")] // unix path
    [DataRow(@"a\SonarLint.xml")]
    public void SetParameterValues_WithValidSonarLintPath_PopulatesProperties(string filePath)
    {
        var compilation = CreateCompilationWithOption(filePath, SourceText.From(File.ReadAllText(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml")));
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.Maximum.Should().Be(1); // Value from the xml file
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithIntParameterType_PopulatesProperties()
    {
        var compilation = CreateCompilationWithOption(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml");
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.Maximum.Should().Be(1); // Value from the xml file
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithStringParameterType_PopulatesProperty()
    {
        var parameterValue = "1";
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S2342", "flagsAttributeFormat", parameterValue);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new EnumNameShouldFollowRegex(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.FlagsEnumNamePattern.Should().Be(parameterValue); // value from XML file
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithTextParameterType_PopulatesProperty()
    {
        var parameterValue = "// Copyright (c) Contoso";
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S1451", "headerFormat", parameterValue);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new CheckFileLicense(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.HeaderFormat.Should().Be(parameterValue); // value from XML file
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithRegularExpressionParameterType_PopulatesProperty()
    {
        var parameterValue = "^m?Logger$";
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S6669", "format", parameterValue);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new LoggerMembersNamesShouldComply(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.Format.Should().Be(parameterValue); // value from XML file
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithFloatParameterType_PopulatesProperty()
    {
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S6418", "randomnessSensibility", "2.5");
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new DoNotHardcodeSecrets(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.RandomnessSensibility.Should().Be(2.5); // value from XML file
    }

    [TestMethod]
    [DataRow("fooBar")]
    [DataRow("2,5")] // Comma decimal separator is not invariant culture
    public void SetParameterValues_SonarLintFileWithStringInsteadOfFloatParameterType_DoesNotPopulateProperty(string parameterValue)
    {
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S6418", "randomnessSensibility", parameterValue);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new DoNotHardcodeSecrets(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.RandomnessSensibility.Should().Be(3); // Default value
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithBooleanParameterType_PopulatesProperty()
    {
        var parameterValue = true;
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S1451", "isRegularExpression", parameterValue.ToString());
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new CheckFileLicense(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.IsRegularExpression.Should().Be(parameterValue); // value from XML file
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithoutRuleParameters_DoesNotPopulateProperties()
    {
        var compilation = CreateCompilationWithOption(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml");
        var analyzer = new LineLength(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.Maximum.Should().Be(200); // Default value
    }

    [TestMethod]
    public void SetParameterValues_CalledTwiceAfterChangeInConfigFile_UpdatesProperties()
    {
        var maxValue = 1;
        List<SonarLintXmlRule> ruleParameters =
        [
            new()
            {
                Key = "S1067",
                Parameters = [new() { Key = "max", Value = maxValue.ToString() }]
            }
        ];
        var sonarLintXml = AnalysisScaffolding.GenerateSonarLintXmlContent(rulesParameters: ruleParameters);
        var filePath = TestFiles.WriteFile(TestContext, "SonarLint.xml", sonarLintXml);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.Maximum.Should().Be(maxValue);

        // Modify the in-memory additional file
        maxValue = 42;
        ruleParameters[0].Parameters[0].Value = maxValue.ToString();
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
        var compilation = CreateCompilationWithOption(@"fakePath\SonarLint.xml", SourceText.From(sonarLintXmlContent));
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.Maximum.Should().Be(3); // Default value
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithStringInsteadOfIntParameterType_PopulatesProperty()
    {
        var parameterValue = "fooBar";
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S1067", "max", parameterValue);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new ExpressionComplexity(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.Maximum.Should().Be(3); // Default value
    }

    [TestMethod]
    public void SetParameterValues_SonarLintFileWithStringInsteadOfBooleanParameterType_PopulatesProperty()
    {
        var parameterValue = "fooBar";
        var filePath = GenerateSonarLintXmlWithParametrizedRule("S1451", "isRegularExpression", parameterValue);
        var compilation = CreateCompilationWithOption(filePath);
        var analyzer = new CheckFileLicense(); // Cannot use mock because we use reflection to find properties.

        ParameterLoader.SetParameterValues(analyzer, compilation.SonarLintXml());
        analyzer.IsRegularExpression.Should().BeFalse(); // Default value
    }

    [TestMethod]
    public void PropertyType_AllMembersAreConvertedByParameterLoader()
    {
        // Guard: every PropertyType member needs a matching case in ParameterLoader.TryConvertToParameterType, otherwise
        // overrides of that type are silently ignored (NET-4546). When adding a member, add a sample value here.
        var samples = new Dictionary<PropertyType, string>
        {
            { PropertyType.String, "a string" },
            { PropertyType.Text, "a text" },
            { PropertyType.Boolean, "true" },
            { PropertyType.Integer, "42" },
            { PropertyType.Float, "2.5" },
            { PropertyType.RegularExpression, "^a$" },
        };

        foreach (PropertyType type in Enum.GetValues(typeof(PropertyType))) // Enum.GetValues<PropertyType>() is not available on net48
        {
            samples.Should().ContainKey(type, "every PropertyType needs a sample value and a ParameterLoader case");
            ParameterLoader.TryConvertToParameterType(samples[type], type, out _).Should().BeTrue("{0} must be converted by ParameterLoader", type);
        }
    }

    [TestMethod]
    public void TryConvertToParameterType_WithUnknownParameterType_Throws()
    {
        var convert = () => ParameterLoader.TryConvertToParameterType("a value", (PropertyType)42, out _);

        convert.Should().Throw<UnexpectedValueException>().WithMessage("Unexpected type value: 42");
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
        List<SonarLintXmlRule> ruleParameters =
        [
            new()
            {
                Key = ruleId,
                Parameters = [new() { Key = key, Value = value }]
            }
        ];
        return AnalysisScaffolding.CreateSonarLintXml(TestContext, rulesParameters: ruleParameters);
    }
}
