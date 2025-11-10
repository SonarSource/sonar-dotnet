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

using System.Xml;

namespace SonarAnalyzer.Core.Test.Common;

[TestClass]
public class RuleLoaderTest
{
    [TestMethod]
    public void GivenNonXmlFile_RuleLoader_Throws()
    {
        var sut = new RuleLoader();
        sut.Invoking(x => x.GetEnabledRules("not xml")).Should().Throw<XmlException>();
    }

    [TestMethod]
    public void GivenSonarLintXml_RuleLoader_LoadsActiveRules()
    {
        const string content = """
            <?xml version="1.0" encoding="UTF-8"?>
            <AnalysisInput>
              <Rules>
                <Rule>
                  <Key>S1067</Key>
                </Rule>
                <Rule>
                </Rule>
              </Rules>
            </AnalysisInput>
            """;

        CollectionAssert.AreEqual(new RuleLoader().GetEnabledRules(content).ToArray(), new[] {"S1067"});
    }
}
