﻿/*
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

using System.Xml;

namespace SonarAnalyzer.Test.Common
{
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
            const string content = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AnalysisInput>
  <Rules>
    <Rule>
      <Key>S1067</Key>
    </Rule>
    <Rule>
    </Rule>
  </Rules>
</AnalysisInput>";

            CollectionAssert.AreEqual(new RuleLoader().GetEnabledRules(content).ToArray(),
                new[] {"S1067"});
        }
    }
}
