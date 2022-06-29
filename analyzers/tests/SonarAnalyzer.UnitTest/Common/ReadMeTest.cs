/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

#if NET // net48 build doesn't set TestContext.DeploymentDirectory correctly

using System.IO;
using System.Text.RegularExpressions;
using SonarAnalyzer.Common;
using SonarAnalyzer.Utilities;

namespace SonarAnalyzer.UnitTest.Common;

[TestClass]
public class ReadMeTest
{
    public TestContext TestContext { get; set; }

    private string readMe;

    [TestInitialize]
    public void Init() =>
       readMe = File.ReadAllText(Path.Combine(TestContext.DeploymentDirectory, @".\..\..\..\..\..\..\README.md"));

    [TestMethod]
    public void HasCorrectRuleCount_CS() =>
        HasCorrectRuleCount(AnalyzerLanguage.CSharp, "C#");

    [TestMethod]
    public void HasCorrectRuleCount_VB() =>
        HasCorrectRuleCount(AnalyzerLanguage.VisualBasic, "VB.&#8203;NET");

    private void HasCorrectRuleCount(AnalyzerLanguage language, string name)
    {
        var rules = RuleFinder.GetAnalyzerTypes(language).Count();
        var match = Regex.Match(readMe, $@"\[(?<count>\d+)\+ {name} rules\]");
        match.Success.Should().BeTrue();

        var count = int.Parse(match.Groups["count"].Value);
        var min = (count / 10) * 10;
        rules.Should().BeInRange(min, min + 10);
    }
}

#endif
