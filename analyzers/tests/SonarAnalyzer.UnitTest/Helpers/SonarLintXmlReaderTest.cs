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
    [DataRow(LanguageNames.CSharp, "cs", ".cs")]
    [DataRow(LanguageNames.VisualBasic, "vbnet", ".vb")]
    public void SonarLintXml_WhenAllValuesAreSet_ExpectedValues(string language, string propertyLanguage, string fileExtension)
    {
        var sut = CreateSonarLintXmlReader($"ResourceTests\\SonarLintXml\\All_Properties_{propertyLanguage}\\SonarLint.xml", language);
        sut.Settings.IgnoreHeaderComments.Should().BeTrue();
        sut.Settings.AnalyzeGeneratedCode.Should().BeFalse();
        sut.Settings.IgnoreIssues.Should().BeFalse();
        sut.Settings.Suffixes.Should().Be(fileExtension);
        sut.Settings.RelativeRootFromSonarLintXml.Should().BeEquivalentTo("Relative/Root/From/SonarLint.xml");
        TestArray(sut.Settings.Exclusions, nameof(sut.Settings.Exclusions));
        TestArray(sut.Settings.Inclusions, nameof(sut.Settings.Inclusions));
        TestArray(sut.Settings.GlobalExclusions, nameof(sut.Settings.GlobalExclusions));
        TestArray(sut.Settings.TestExclusions, nameof(sut.Settings.TestExclusions));
        TestArray(sut.Settings.TestInclusions, nameof(sut.Settings.TestInclusions));

        static void TestArray(string[] array, string folder)
        {
            array.Should().HaveCount(2);
            array[0].Should().BeEquivalentTo($"Fake/{folder}/**/*");
            array[1].Should().BeEquivalentTo($"Fake/{folder}/Second*/**/*");
        }
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("this is not an xml")]
    [DataRow(@"<?xml version=""1.0"" encoding=""UTF - 8""?><AnalysisInput><Settings>")]
    public void SonarLintXml_WithMalformedXml_DefaultBehaviour(string sonarLintXmlContent) =>
        CheckSonarLintXmlDefaultValues(new SonarLintXmlReader(SourceText.From(sonarLintXmlContent), LanguageNames.CSharp));

    [TestMethod]
    public void SonarLintXmlSettings_MissingProperties_DefaultBehaviour() =>
        CheckSonarLintXmlDefaultValues(CreateSonarLintXmlReader("ResourceTests\\SonarLintXml\\Missing_properties\\SonarLint.xml"));

    [TestMethod]
    public void SonarLintXmlSettings_WithIncorrectValueType_DefaultBehaviour() =>
        CheckSonarLintXmlDefaultValues(CreateSonarLintXmlReader("ResourceTests\\SonarLintXml\\Incorrect_value_type\\SonarLint.xml"));

    private static void CheckSonarLintXmlDefaultValues(SonarLintXmlReader sut)
    {
        sut.Settings.AnalyzeGeneratedCode.Should().BeFalse();
        sut.Settings.IgnoreHeaderComments.Should().BeFalse();
        sut.Settings.IgnoreIssues.Should().BeFalse();
        sut.Settings.Suffixes.Should().BeEmpty();
        sut.Settings.RelativeRootFromSonarLintXml.Should().BeEmpty();
        sut.Settings.Exclusions.Should().NotBeNull().And.HaveCount(0);
        sut.Settings.Inclusions.Should().NotBeNull().And.HaveCount(0);
        sut.Settings.GlobalExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.Settings.TestExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.Settings.TestInclusions.Should().NotBeNull().And.HaveCount(0);
    }

    private static SonarLintXmlReader CreateSonarLintXmlReader(string relativePath, string language = LanguageNames.CSharp) =>
        new(SourceText.From(File.ReadAllText(relativePath)), language);
}
