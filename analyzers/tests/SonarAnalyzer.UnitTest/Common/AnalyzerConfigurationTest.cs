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

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static SonarAnalyzer.Common.AnalyzerConfiguration;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class AnalyzerConfigurationTest
    {
        private const string FirstSonarLintFile = @"bar\SonarLint.xml";
        private const string FirstRuleId = "S0000";
        private const string SecondSonarLintFile = @"qix\SonarLint.xml";
        private const string SecondRuleId = "S9999";
        private Mock<IRuleLoader> ruleLoaderMock;

        [TestInitialize]
        public void Initialize()
        {
            ruleLoaderMock = new Mock<IRuleLoader>(MockBehavior.Strict);
            ruleLoaderMock.Setup(r => r.GetEnabledRules(FirstSonarLintFile)).Returns(new HashSet<string>() { FirstRuleId });
            ruleLoaderMock.Setup(r => r.GetEnabledRules(SecondSonarLintFile)).Returns(new HashSet<string>() { SecondRuleId });
        }

        [TestMethod]
        public void AlwaysEnabled_WhenNotInitialized_ReturnsTrue()
        {
            var sut = AlwaysEnabled;
            IsTrue(sut.IsEnabled("S101"));
        }

        [TestMethod]
        public void AlwaysEnabled_AnyValue_ReturnsTrue()
        {
            var sut = AlwaysEnabled;
            IsTrue(sut.IsEnabled(null));
            IsTrue(sut.IsEnabled(""));
            IsTrue(sut.IsEnabled("foo"));
        }

        [TestMethod]
        public void AlwaysEnabled_IgnoresInitialize()
        {
            var sut = AlwaysEnabled;

            sut.Initialize(null);

            IsTrue(sut.IsEnabled(FirstRuleId));
        }

        [TestMethod]
        public void HotspotConfiguration_WhenInitializeIsCalledWithDifferentSonarLintPaths_UpdatesEnabledRules()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);

            // act
            Initialize(sut, FirstSonarLintFile);

            // assert
            IsTrue(sut.IsEnabled(FirstRuleId));
            IsFalse(sut.IsEnabled(SecondRuleId));

            // act
            Initialize(sut, SecondSonarLintFile);

            // assert
            IsFalse(sut.IsEnabled(FirstRuleId));
            IsTrue(sut.IsEnabled(SecondRuleId));

            ruleLoaderMock.Verify(r => r.GetEnabledRules(FirstSonarLintFile), Times.Once);
            ruleLoaderMock.Verify(r => r.GetEnabledRules(SecondSonarLintFile), Times.Once);
        }

        [TestMethod]
        public void HotspotConfiguration_WhenInitializedTwiceWithTheSameFile_DoesNotUpdateEnabledRules()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);

            // act
            Initialize(sut, FirstSonarLintFile);
            Initialize(sut, FirstSonarLintFile);

            // assert
            ruleLoaderMock.Verify(r => r.GetEnabledRules(It.IsAny<string>()), Times.Once);
            ruleLoaderMock.Verify(r => r.GetEnabledRules(FirstSonarLintFile), Times.Once);
        }

        [TestMethod]
        public void HotspotConfiguration_WhenInitializeIsSecondTimeWithNonSonarLint_DoesNotUpdateEnabledRules()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);

            // act
            Initialize(sut, FirstSonarLintFile);
            Initialize(sut, "Foo.xml");

            // assert
            ruleLoaderMock.Verify(r => r.GetEnabledRules(It.IsAny<string>()), Times.Once);
            ruleLoaderMock.Verify(r => r.GetEnabledRules(FirstSonarLintFile), Times.Once);
        }

        [TestMethod]
        public void HotspotConfiguration_WhenIsEnabledWithoutInitialized_ThrowException()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);
            sut.Invoking(x => x.IsEnabled("")).Should().Throw<InvalidOperationException>().WithMessage("Call Initialize() before calling IsEnabled().");
        }

        [TestMethod]
        public void HotspotConfiguration_WhenIsInitializedWithNull_ThrowsException()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);
            sut.Invoking(x => x.Initialize(null)).Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void HotspotConfiguration_GivenDifferentFileName_WillNotFinishInitialization()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);

            Initialize(sut, "FooBarSonarLint.xml");

            ruleLoaderMock.Verify(r => r.GetEnabledRules(It.IsAny<string>()), Times.Never);
        }

        private void Initialize(HotspotConfiguration sut, string path) =>
            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(path)));

        private ImmutableArray<AdditionalText> GetAdditionalFiles(string path) =>
            ImmutableArray.Create(Mock.Of<AdditionalText>(additionalText => additionalText.Path == path));
    }
}
