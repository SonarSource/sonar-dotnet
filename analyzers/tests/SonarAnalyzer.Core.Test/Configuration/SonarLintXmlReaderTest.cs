/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Configuration.Test;

[TestClass]
public class SonarLintXmlReaderTest
{
    [TestMethod]
    [DataRow(LanguageNames.CSharp, "cs")]
    [DataRow(LanguageNames.VisualBasic, "vbnet")]
    public void SonarLintXmlReader_WhenAllValuesAreSet_ExpectedValues(string language, string xmlLanguageName)
    {
        var sut = CreateSonarLintXmlReader(@$"TestResources\SonarLintXml\All_Properties_{xmlLanguageName}\SonarLint.xml");
        sut.IgnoreHeaderComments(language).Should().BeTrue();
        sut.AnalyzeGeneratedCode(language).Should().BeFalse();
        sut.AnalyzeRazorCode(language).Should().BeFalse();
        AssertArrayContent(sut.Exclusions, nameof(sut.Exclusions));
        AssertArrayContent(sut.Inclusions, nameof(sut.Inclusions));
        AssertArrayContent(sut.GlobalExclusions, nameof(sut.GlobalExclusions));
        AssertArrayContent(sut.TestExclusions, nameof(sut.TestExclusions));
        AssertArrayContent(sut.TestInclusions, nameof(sut.TestInclusions));
        AssertArrayContent(sut.GlobalTestExclusions, nameof(sut.GlobalTestExclusions));

        sut.ParametrizedRules.Should().HaveCount(2);
        var rule = sut.ParametrizedRules.First(x => x.Key.Equals("S2342"));
        rule.Parameters[0].Key.Should().Be("format");
        rule.Parameters[0].Value.Should().Be("^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$");
        rule.Parameters[1].Key.Should().Be("flagsAttributeFormat");
        rule.Parameters[1].Value.Should().Be("^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?s$");

        static void AssertArrayContent(string[] array, string folder)
        {
            array.Should().HaveCount(2);
            array[0].Should().BeEquivalentTo($"Fake/{folder}/**/*");
            array[1].Should().BeEquivalentTo($"Fake/{folder}/Second*/**/*");
        }
    }

    [TestMethod]
    public void SonarLintXmlReader_PartiallyMissingProperties_ExpectedAndDefaultValues()
    {
        var sut = CreateSonarLintXmlReader(@"TestResources\SonarLintXml\Partially_missing_properties\SonarLint.xml");
        sut.IgnoreHeaderComments(LanguageNames.CSharp).Should().BeFalse();
        sut.AnalyzeGeneratedCode(LanguageNames.CSharp).Should().BeTrue();
        sut.AnalyzeRazorCode(LanguageNames.CSharp).Should().BeTrue();
        AssertArrayContent(sut.Exclusions, nameof(sut.Exclusions));
        AssertArrayContent(sut.Inclusions, nameof(sut.Inclusions));
        sut.GlobalExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.TestExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.TestInclusions.Should().NotBeNull().And.HaveCount(0);
        sut.GlobalTestExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.ParametrizedRules.Should().NotBeNull().And.HaveCount(0);
    }

    [TestMethod]
    public void SonarLintXmlReader_PropertiesCSharpTrueVBNetFalse_ExpectedValues()
    {
        var sut = CreateSonarLintXmlReader(@"TestResources\SonarLintXml\PropertiesCSharpTrueVbnetFalse\SonarLint.xml");
        sut.IgnoreHeaderComments(LanguageNames.CSharp).Should().BeTrue();
        sut.IgnoreHeaderComments(LanguageNames.VisualBasic).Should().BeFalse();
        sut.AnalyzeGeneratedCode(LanguageNames.CSharp).Should().BeTrue();
        sut.AnalyzeGeneratedCode(LanguageNames.VisualBasic).Should().BeFalse();
        sut.AnalyzeRazorCode(LanguageNames.CSharp).Should().BeTrue();
        sut.AnalyzeRazorCode(LanguageNames.VisualBasic).Should().BeFalse();
    }

    [TestMethod]
    public void SonarLintXmlReader_DuplicatedProperties_DoesNotFail() =>
        ((Action)(() => CreateSonarLintXmlReader(@"TestResources\SonarLintXml\Duplicated_Properties\SonarLint.xml"))).Should().NotThrow();

    [TestMethod]
    [DataRow("")]
    [DataRow("this is not an xml")]
    [DataRow(@"<?xml version=""1.0"" encoding=""UTF - 8""?><AnalysisInput><Settings>")]
    public void SonarLintXmlReader_WithMalformedXml_DefaultBehaviour(string sonarLintXmlContent) =>
        CheckSonarLintXmlReaderDefaultValues(new SonarLintXmlReader(SourceText.From(sonarLintXmlContent)));

    [TestMethod]
    public void SonarLintXmlReader_MissingProperties_DefaultBehaviour() =>
        CheckSonarLintXmlReaderDefaultValues(CreateSonarLintXmlReader(@"TestResources\SonarLintXml\Missing_properties\SonarLint.xml"));

    [TestMethod]
    public void SonarLintXmlReader_WithIncorrectValueType_DefaultBehaviour() =>
        CheckSonarLintXmlReaderDefaultValues(CreateSonarLintXmlReader(@"TestResources\SonarLintXml\Incorrect_value_type\SonarLint.xml"));

    [TestMethod]
    public void SonarLintXmlReader_CheckEmpty_DefaultBehaviour() =>
        CheckSonarLintXmlReaderDefaultValues(SonarLintXmlReader.Empty);

    [TestMethod]
    public void SonarLintXmlReader_LanguageDoesNotExist_Throws()
    {
        var sut = CreateSonarLintXmlReader(@"TestResources\SonarLintXml\All_Properties_cs\SonarLint.xml");
        sut.Invoking(x => x.IgnoreHeaderComments(LanguageNames.FSharp)).Should().Throw<UnexpectedLanguageException>().WithMessage("Unexpected language: F#");
        sut.Invoking(x => x.AnalyzeGeneratedCode(LanguageNames.FSharp)).Should().Throw<UnexpectedLanguageException>().WithMessage("Unexpected language: F#");
        sut.Invoking(x => x.AnalyzeRazorCode(LanguageNames.FSharp)).Should().Throw<UnexpectedLanguageException>().WithMessage("Unexpected language: F#");
    }

    private static void CheckSonarLintXmlReaderDefaultValues(SonarLintXmlReader sut)
    {
        sut.AnalyzeGeneratedCode(LanguageNames.CSharp).Should().BeFalse();
        sut.IgnoreHeaderComments(LanguageNames.CSharp).Should().BeFalse();
        sut.AnalyzeRazorCode(LanguageNames.CSharp).Should().BeTrue();
        sut.Exclusions.Should().NotBeNull().And.HaveCount(0);
        sut.Inclusions.Should().NotBeNull().And.HaveCount(0);
        sut.GlobalExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.TestExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.TestInclusions.Should().NotBeNull().And.HaveCount(0);
        sut.GlobalTestExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.ParametrizedRules.Should().NotBeNull().And.HaveCount(0);
    }

    private static void AssertArrayContent(string[] array, string folder)
    {
        array.Should().HaveCount(2);
        array[0].Should().BeEquivalentTo($"Fake/{folder}/**/*");
        array[1].Should().BeEquivalentTo($"Fake/{folder}/Second*/**/*");
    }

    private static SonarLintXmlReader CreateSonarLintXmlReader(string relativePath) =>
        new(SourceText.From(File.ReadAllText(relativePath)));
}
