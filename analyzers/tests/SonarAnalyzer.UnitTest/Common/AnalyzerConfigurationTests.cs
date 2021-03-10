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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using FluentAssertions;
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
        public void HotspotConfiguration_WhenInitializeIsCalledWithDifferentPathAndDifferentLoaders_EnabledRulesAreUpdated()
        {
            var sut = new AnalyzerConfiguration.HotspotConfiguration();

            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(@"bar\SonarLint.xml")), GetRuleLoader("S1067"));
            Assert.IsTrue(sut.IsEnabled("S1067"));
            Assert.IsFalse(sut.IsEnabled("S9999"));

            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(@"qix\SonarLint.xml")), GetRuleLoader("S9999"));
            Assert.IsFalse(sut.IsEnabled("S1067"));
            Assert.IsTrue(sut.IsEnabled("S9999"));
        }

        [TestMethod]
        public void HotspotConfiguration_WhenInitializeIsCalledWithTheSamePathAndDifferentLoaders_EnabledRulesAreNotUpdated()
        {
            var sut = new AnalyzerConfiguration.HotspotConfiguration();
            var path = "SonarLint.xml";

            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(path)), GetRuleLoader("S1067"));
            Assert.IsTrue(sut.IsEnabled("S1067"));
            Assert.IsFalse(sut.IsEnabled("S9999"));

            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(path)), GetRuleLoader("S9999"));
            Assert.IsTrue(sut.IsEnabled("S1067"));
            Assert.IsFalse(sut.IsEnabled("S9999"));
        }

        [TestMethod]
        public void HotspotConfiguration_WhenInitializeIsSecondTimeWithDifferentFileName_EnabledRulesAreNotUpdated()
        {
            var sut = new AnalyzerConfiguration.HotspotConfiguration();

            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(@"bar\SonarLint.xml")), GetRuleLoader("S1067"));
            Assert.IsTrue(sut.IsEnabled("S1067"));
            Assert.IsFalse(sut.IsEnabled("S9999"));

            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(@"qix\Foo.xml")), GetRuleLoader("S9999"));
            Assert.IsTrue(sut.IsEnabled("S1067"));
            Assert.IsFalse(sut.IsEnabled("S9999"));
        }

        [TestMethod]
        public void HotspotConfiguration_WhenIsEnabledWithoutInitialized_ThrowException()
        {
            var sut = new AnalyzerConfiguration.HotspotConfiguration();
            sut.Invoking(x => x.IsEnabled("")).Should().Throw<InvalidOperationException>().WithMessage("Call Initialize() before calling IsEnabled().");
        }

        [TestMethod]
        public void HotspotConfiguration_GivenDifferentFileName_WillNotFinishInitialization()
        {
            var sut = new AnalyzerConfiguration.HotspotConfiguration();
            var ruleLoaderMock = GetEmptyMock();

            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(@"foo\SomeFile.xml")), ruleLoaderMock.Object);

            ruleLoaderMock.Verify(r => r.GetEnabledRules(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void HotspotConfiguration_WhenInitializedTwiceWithTheSameFile_WillGetEnabledRulesOnlyOnce()
        {
            var sut = new AnalyzerConfiguration.HotspotConfiguration();
            var ruleLoaderMock = GetEmptyMock();

            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(@"foo\SonarLint.xml")), ruleLoaderMock.Object);
            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(@"foo\SonarLint.xml")), ruleLoaderMock.Object);
            ruleLoaderMock.Verify(r => r.GetEnabledRules(It.IsAny<string>()), Times.Once);
        }

        private ImmutableArray<AdditionalText> GetAdditionalFiles(string path) =>
            ImmutableArray.Create(Mock.Of<AdditionalText>(additionalText => additionalText.Path == path));

        private IRuleLoader GetRuleLoader(string rule) =>
            Mock.Of<IRuleLoader>(loader => loader.GetEnabledRules(It.IsAny<string>()) == new HashSet<string>() { rule });

        private Mock<IRuleLoader> GetEmptyMock()
        {
            var ruleLoaderMock = new Mock<IRuleLoader>();
            ruleLoaderMock.Setup(r => r.GetEnabledRules(It.IsAny<string>())).Returns(new HashSet<string>());
            return ruleLoaderMock;
        }
    }
}
