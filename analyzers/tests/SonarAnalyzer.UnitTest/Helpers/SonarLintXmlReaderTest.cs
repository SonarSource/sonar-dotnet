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

namespace SonarAnalyzer.UnitTest.Helpers;

[TestClass]
public class SonarLintXmlReaderTest
{
    [DataTestMethod]
    [DataRow(LanguageNames.CSharp, "cs")]
    [DataRow(LanguageNames.VisualBasic, "vbnet")]
    public void SonarLintXmlReader_WhenAllValuesAreSet_ExpectedValues(string language, string propertyLanguage)
    {
        var sut = CreateSonarLintXmlReader($"ResourceTests\\SonarLintXml\\All_Properties_{propertyLanguage}\\SonarLint.xml", language);
        sut.IgnoreHeaderComments.Should().BeTrue();
        sut.AnalyzeGeneratedCode.Should().BeFalse();
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
        var sut = CreateSonarLintXmlReader("ResourceTests\\SonarLintXml\\Partially_missing_properties\\SonarLint.xml");
        sut.IgnoreHeaderComments.Should().BeFalse();
        sut.AnalyzeGeneratedCode.Should().BeTrue();
        AssertArrayContent(sut.Exclusions, nameof(sut.Exclusions));
        AssertArrayContent(sut.Inclusions, nameof(sut.Inclusions));
        sut.GlobalExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.TestExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.TestInclusions.Should().NotBeNull().And.HaveCount(0);
        sut.GlobalTestExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.ParametrizedRules.Should().NotBeNull().And.HaveCount(0);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("this is not an xml")]
    [DataRow(@"<?xml version=""1.0"" encoding=""UTF - 8""?><AnalysisInput><Settings>")]
    public void SonarLintXmlReader_WithMalformedXml_DefaultBehaviour(string sonarLintXmlContent) =>
        CheckSonarLintXmlReaderDefaultValues(new SonarLintXmlReader(SourceText.From(sonarLintXmlContent), LanguageNames.CSharp));

    [TestMethod]
    public void SonarLintXmlReader_MissingProperties_DefaultBehaviour() =>
        CheckSonarLintXmlReaderDefaultValues(CreateSonarLintXmlReader("ResourceTests\\SonarLintXml\\Missing_properties\\SonarLint.xml"));

    [TestMethod]
    public void SonarLintXmlReader_WithIncorrectValueType_DefaultBehaviour() =>
        CheckSonarLintXmlReaderDefaultValues(CreateSonarLintXmlReader("ResourceTests\\SonarLintXml\\Incorrect_value_type\\SonarLint.xml"));

    [TestMethod]
    public void SonarLintXmlReader_CheckEmpty_DefaultBehaviour() =>
        CheckSonarLintXmlReaderDefaultValues(SonarLintXmlReader.Empty);

    private static void CheckSonarLintXmlReaderDefaultValues(SonarLintXmlReader sut)
    {
        sut.AnalyzeGeneratedCode.Should().BeFalse();
        sut.IgnoreHeaderComments.Should().BeFalse();
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

    private static void AssertArrayContent(string[] array, string folder)
    {
        array.Should().HaveCount(2);
        array[0].Should().BeEquivalentTo($"Fake/{folder}/**/*");
        array[1].Should().BeEquivalentTo($"Fake/{folder}/Second*/**/*");
    }

    private static SonarLintXmlReader CreateSonarLintXmlReader(string relativePath, string language = LanguageNames.CSharp) =>
        new(SourceText.From(File.ReadAllText(relativePath)), language);
}
