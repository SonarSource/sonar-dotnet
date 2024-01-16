/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
using System.Xml.Serialization;

namespace SonarAnalyzer.Test.Helpers;

[TestClass]
public class SonarLintXmlTest
{
    [TestMethod]
    public void SonarLintXml_DeserializeFile_ExpectedValues()
    {
        var deserializer = new XmlSerializer(typeof(SonarLintXml));
        using TextReader textReader = new StreamReader(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml");
        var sonarLintXml = (SonarLintXml)deserializer.Deserialize(textReader);

        AssertSettings(sonarLintXml.Settings);
        AssertRules(sonarLintXml.Rules);
    }

    private static void AssertSettings(List<SonarLintXmlKeyValuePair> settings)
    {
        settings.Should().HaveCount(11);

        AssertKeyValuePair(settings[0], "sonar.cs.analyzeRazorCode", "false");
        AssertKeyValuePair(settings[1], "sonar.cs.ignoreHeaderComments", "true");
        AssertKeyValuePair(settings[2], "sonar.cs.analyzeGeneratedCode", "false");
        AssertKeyValuePair(settings[3], "sonar.cs.file.suffixes", ".cs");
        AssertKeyValuePair(settings[4], "sonar.cs.roslyn.ignoreIssues", "false");
        AssertKeyValuePair(settings[5], "sonar.exclusions", "Fake/Exclusions/**/*,Fake/Exclusions/Second*/**/*");
        AssertKeyValuePair(settings[6], "sonar.inclusions", "Fake/Inclusions/**/*,Fake/Inclusions/Second*/**/*");
        AssertKeyValuePair(settings[7], "sonar.global.exclusions", "Fake/GlobalExclusions/**/*,Fake/GlobalExclusions/Second*/**/*");
        AssertKeyValuePair(settings[8], "sonar.test.exclusions", "Fake/TestExclusions/**/*,Fake/TestExclusions/Second*/**/*");
        AssertKeyValuePair(settings[9], "sonar.test.inclusions", "Fake/TestInclusions/**/*,Fake/TestInclusions/Second*/**/*");
        AssertKeyValuePair(settings[10], "sonar.global.test.exclusions", "Fake/GlobalTestExclusions/**/*,Fake/GlobalTestExclusions/Second*/**/*");
    }

    private static void AssertRules(List<SonarLintXmlRule> rules)
    {
        rules.Should().HaveCount(4);
        rules.Where(x => x.Parameters.Any()).Should().HaveCount(2);

        rules[0].Key.Should().BeEquivalentTo("S2225");
        rules[0].Parameters.Should().BeEmpty();

        rules[1].Key.Should().BeEquivalentTo("S2342");
        rules[1].Parameters.Should().HaveCount(2);
        AssertKeyValuePair(rules[1].Parameters[0], "format", "^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$");
        AssertKeyValuePair(rules[1].Parameters[1], "flagsAttributeFormat", "^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?s$");

        rules[2].Key.Should().BeEquivalentTo("S2346");
        rules[2].Parameters.Should().BeEmpty();

        rules[3].Key.Should().BeEquivalentTo("S1067");
        rules[3].Parameters.Should().HaveCount(1);
        AssertKeyValuePair(rules[3].Parameters[0], "max", "1");
    }

    private static void AssertKeyValuePair(SonarLintXmlKeyValuePair pair, string expectedKey, string expectedValue)
    {
        pair.Key.Should().BeEquivalentTo(expectedKey);
        pair.Value.Should().BeEquivalentTo(expectedValue);
    }
}
