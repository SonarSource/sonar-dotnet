/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class AnalyzerConfigurationTests
    {
        [TestMethod]
        public void GivenSonarLintXmlIsLoaded_WhenInitializeIsCalledWithDifferentPath_EnabledRulesAreUpdated()
        {
            var sut = AnalyzerConfiguration.Hotspot;
            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(@"a\SonarLint.xml")), GetRuleLoader("S1067"));
            Assert.IsTrue(sut.IsEnabled("S1067"));

            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(@"b\SonarLint.xml")), GetRuleLoader("S1068"));
            Assert.IsFalse(sut.IsEnabled("S1067"));
            Assert.IsTrue(sut.IsEnabled("S1068"));
        }

        private ImmutableArray<AdditionalText> GetAdditionalFiles(string path) =>
            ImmutableArray.Create(Mock.Of<AdditionalText>(additionalText => additionalText.Path == path));

        private IRuleLoader GetRuleLoader(string rule) =>
            Mock.Of<IRuleLoader>(loader => loader.GetEnabledRules(It.IsAny<string>()) == new HashSet<string>() { rule });
    }
}
